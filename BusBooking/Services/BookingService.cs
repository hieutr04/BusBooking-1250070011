using System.Data;
using System.Security.Cryptography;
using BusBooking.Data;
using BusBooking.Models;
using BusBooking.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace BusBooking.Services;
public class BookingService(AppDbContext db,DbGate gate,RealtimePublisher realtime,IOptions<BookingOptions> config) {
    private readonly BookingOptions options=config.Value;
    private static bool SameSeats(IEnumerable<int> a,IEnumerable<int> b)=>a.Order().SequenceEqual(b.Order());
    private static void Owner(SeatHold h,string actor)=>Guard.Rule(h.OwnerId==actor,"Bạn không sở hữu mã giữ chỗ này.");
    private async Task<Booking> LoadBooking(Guid id,CancellationToken ct)=>Guard.Found(await db.Bookings.Include(x=>x.Tickets).ThenInclude(x=>x.Seat).Include(x=>x.Payments).SingleOrDefaultAsync(x=>x.Id==id,ct));
    private async Task<List<Trip>> LockTrips(IEnumerable<int> ids,CancellationToken ct) {
        var list=new List<Trip>();foreach(var id in ids.Distinct().Order())list.Add(await gate.Trip(id,ct));return list;
    }
    private async Task<int> BookingTrip(Guid id,CancellationToken ct)=>await db.Bookings.Where(x=>x.Id==id).Select(x=>(int?)x.Hold.TripId).SingleOrDefaultAsync(ct)??throw new BusinessException(404,"Không tìm thấy đặt vé.");
    private async Task<SeatHold> Hold(Guid id,CancellationToken ct)=>Guard.Found(await db.SeatHolds.Include(x=>x.Items).SingleOrDefaultAsync(x=>x.Id==id,ct));
    // Caller already owns the trip gate. Every writer uses this gate, then ordered seat locks.
    private async Task<int> ExpireUnderGate(Trip trip,DateTime now,CancellationToken ct,IEnumerable<int>? targetedSeats=null) {
        var query=db.SeatHolds.Where(x=>x.TripId==trip.Id&&x.Status==HoldStatus.Active&&x.ExpiresAtUtc<=now);
        if(targetedSeats!=null) {var idsToReclaim=targetedSeats.ToArray();query=query.Where(x=>x.Items.Any(i=>idsToReclaim.Contains(i.TripSeatId)));}
        var ordered=query.OrderBy(x=>x.ExpiresAtUtc);
        var holds=await (targetedSeats==null?ordered.Take(options.ExpiryBatchSize):ordered).ToListAsync(ct);
        if(holds.Count==0)return 0;
        var ids=holds.Select(x=>x.Id).ToArray();
        var seatIds=await db.TripSeats.Where(x=>x.TripId==trip.Id&&x.CurrentHoldId!=null&&ids.Contains(x.CurrentHoldId.Value)).Select(x=>x.Id).ToListAsync(ct);
        var seats=await gate.Seats(trip.Id,seatIds,ct);
        // Re-read clock after all lock waits, never revive an expired hold.
        now=await gate.Now(ct);
        foreach(var h in holds.Where(h=>h.ExpiresAtUtc<=now)) {
            foreach(var seat in seats.Where(s=>s.Status==SeatStatus.Held&&s.CurrentHoldId==h.Id)) {seat.Status=SeatStatus.Available;seat.CurrentHoldId=null;}
            h.Status=HoldStatus.Expired;gate.Audit("worker","HoldExpired",h.Id.ToString(),now);
        }
        return holds.Count;
    }
    private async Task<int> ExpirePaymentsUnderGate(Trip trip,DateTime now,CancellationToken ct,int[]? targetedSeats=null) {
        var query=db.Bookings.Where(b=>b.Hold.TripId==trip.Id&&b.Status==BookingStatus.Active&&b.PaymentExpiresUtc<=now);
        if(targetedSeats!=null)
            query=query.Where(b=>b.Tickets.Any(t=>t.IsActive&&targetedSeats.Contains(t.TripSeatId)));
        var ordered=query.OrderBy(b=>b.PaymentExpiresUtc).ThenBy(b=>b.Id);
        var expired=await (targetedSeats==null?ordered.Take(options.ExpiryBatchSize):ordered)
            .Include(b=>b.Tickets.Where(t=>t.IsActive)).ThenInclude(t=>t.Seat).ToListAsync(ct);
        if(expired.Count==0)return 0;

        await gate.Seats(trip.Id,expired.SelectMany(b=>b.Tickets).Select(t=>t.TripSeatId),ct);
        foreach(var booking in expired) {
            CancelBooking(booking);
            booking.Status=BookingStatus.Expired;
            gate.Audit("worker","QrPaymentExpired",booking.Id.ToString(),now);
        }
        return expired.Count;
    }
    public async Task<SeatSnapshot> Snapshot(int tripId,string? actor,CancellationToken ct=default) {
        // A short read transaction with the same trip gate gives a coherent revision + seats snapshot.
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted,ct);
        var t=await gate.Trip(tripId,ct);var now=await gate.Now(ct);
        var seats=await db.TripSeats.AsNoTracking().Where(x=>x.TripId==tripId).OrderBy(x=>x.Floor).ThenBy(x=>x.Row).ThenBy(x=>x.Column)
            .Select(x=>new{x.Id,x.Code,x.Row,x.Column,x.Floor,x.Status,x.CurrentHoldId,Expires=x.CurrentHold==null?(DateTime?)null:x.CurrentHold.ExpiresAtUtc,Owner=x.CurrentHold==null?null:x.CurrentHold.OwnerId}).ToListAsync(ct);
        var awaitingPayment=(await db.Tickets.AsNoTracking().Where(x=>x.Seat.TripId==tripId&&x.IsActive&&
            x.Booking.Status==BookingStatus.Active&&x.Booking.PaymentExpiresUtc!=null)
            .Select(x=>x.TripSeatId).ToListAsync(ct)).ToHashSet();
        List<HoldView> mine=actor==null?[]:await db.SeatHolds.AsNoTracking().Where(x=>x.TripId==tripId&&x.OwnerId==actor&&x.Status==HoldStatus.Active&&x.ExpiresAtUtc>now).Select(x=>new HoldView(x.Id,x.Status.ToString(),x.ExpiresAtUtc,x.Items.Select(i=>i.TripSeatId).ToArray(),x.PickupStopId,x.DropoffStopId)).ToListAsync(ct);
        mine=mine.Select(h=>h with {ExpiresAtUtc=BusinessTime.Utc(h.ExpiresAtUtc)}).ToList();
        var columns=seats.Count==0?1:seats.Max(x=>x.Column);
        var stops=await db.TripStops.AsNoTracking().Where(s=>s.TripId==tripId&&s.IsActive).OrderBy(s=>s.StopOrder).ToListAsync(ct);
        var route=await db.Routes.AsNoTracking().SingleAsync(x=>x.Id==t.BusRouteId,ct);
        var allowed=StopChoicePolicy.ForNew(t,route,stops).Select(x=>x.Id).ToHashSet();
        var retained=mine.SelectMany(h=>new int?[]{h.PickupStopId,h.DropoffStopId}).Where(x=>x.HasValue).Select(x=>x!.Value).ToHashSet();
        var visible=stops.Where(x=>allowed.Contains(x.Id)||retained.Contains(x.Id)).Select(x=>TripStopService.ToView(t,x) with {CanSelect=allowed.Contains(x.Id),LocationSpecific=StopChoicePolicy.Specific(x,route)}).ToList();
        var result=new SeatSnapshot(tripId,t.Revision,now,t.SalesOpen&&t.Status==TripStatus.Scheduled&&t.DepartureUtc>now,t.Fare,columns,
            seats.Select(x=>new SeatView{Id=x.Id,Code=x.Code,Row=x.Row,Column=x.Column,Floor=x.Floor,Status=awaitingPayment.Contains(x.Id)?"Held":x.Status==SeatStatus.Held&&x.Expires<=now?"Available":x.Status.ToString(),Mine=actor!=null&&x.Owner==actor&&x.Status==SeatStatus.Held&&x.Expires>now}).ToList(),mine,visible,options.ArriveEarlyMinutes);
        await tx.CommitAsync(ct);return result;
    }
    public async Task<HoldView> Acquire(string actor,HoldForm form,CancellationToken ct=default) {
        Guard.Input(form.RequestKey!=Guid.Empty,"Thiếu mã thao tác giữ chỗ.");
        Guard.Input(form.SeatIds.Length>0&&form.SeatIds.Length<=options.MaxSeats&&form.SeatIds.Distinct().Count()==form.SeatIds.Length,"Chọn từ 1 đến 6 ghế khác nhau.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted,ct);
        var trip=await gate.Trip(form.TripId,ct);
        var previous=await db.SeatHolds.Include(x=>x.Items).SingleOrDefaultAsync(x=>x.OwnerId==actor&&x.RequestKey==form.RequestKey,ct);
        if(previous!=null) {
            Guard.Rule(previous.TripId==form.TripId&&SameSeats(previous.Items.Select(x=>x.TripSeatId),form.SeatIds),"Mã thao tác đã dùng cho lựa chọn khác.");
            Guard.Rule((!form.PickupStopId.HasValue||form.PickupStopId==previous.PickupStopId)&&
                (!form.DropoffStopId.HasValue||form.DropoffStopId==previous.DropoffStopId),"Mã thao tác đã dùng cho điểm đón/trả khác.");
            await tx.CommitAsync(ct);return View(previous); // exact retry, never extend ExpiresAt
        }
        var seats=await gate.Seats(trip.Id,form.SeatIds,ct);
        var now=await gate.Now(ct);DbGate.Sellable(trip,now);
        await ExpireUnderGate(trip,now,ct,form.SeatIds);
        await ExpirePaymentsUnderGate(trip,now,ct,form.SeatIds);
        Guard.Rule(seats.All(x=>x.Status==SeatStatus.Available),"Một hoặc nhiều ghế vừa được người khác giữ/đặt. Vui lòng chọn lại.");
        now=await gate.Now(ct);DbGate.Sellable(trip,now);
        var pair=await new TripStopService(db,gate).ResolveNew(trip,form.PickupStopId,form.DropoffStopId,ct);
        var hold=new SeatHold{PickupStopId=pair.Pickup.Id,DropoffStopId=pair.Dropoff.Id,Id=Guid.NewGuid(),TripId=trip.Id,OwnerId=actor,RequestKey=form.RequestKey,CreatedUtc=now,ExpiresAtUtc=now.AddSeconds(options.HoldSeconds),Status=HoldStatus.Active};
        db.SeatHolds.Add(hold);
        foreach(var s in seats) {
            hold.Items.Add(new HoldSeat{TripSeatId=s.Id,TripId=trip.Id});s.Status=SeatStatus.Held;s.CurrentHoldId=hold.Id;
        }
        trip.Revision++;gate.Audit(actor,"HoldAcquired",hold.Id.ToString(),now);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        await realtime.Publish(trip);return View(hold);
    }
    private static HoldView View(SeatHold h)=>new(h.Id,h.Status.ToString(),BusinessTime.Utc(h.ExpiresAtUtc),h.Items.Select(x=>x.TripSeatId).Order().ToArray(),h.PickupStopId,h.DropoffStopId);
    public async Task Release(string actor,Guid holdId,CancellationToken ct=default) {
        var tripId=await db.SeatHolds.Where(x=>x.Id==holdId).Select(x=>(int?)x.TripId).SingleOrDefaultAsync(ct)??throw new BusinessException(404,"Mã giữ chỗ không tồn tại.");
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted,ct);
        var trip=await gate.Trip(tripId,ct);var hold=await Hold(holdId,ct);Owner(hold,actor);
        if(hold.Status is HoldStatus.Released or HoldStatus.Expired) {await tx.CommitAsync(ct);return;}
        Guard.Rule(hold.Status==HoldStatus.Active,"Giữ chỗ đã được xác nhận thành vé; không thể nhả lượt giữ cũ.");
        var seats=await gate.Seats(tripId,hold.Items.Select(x=>x.TripSeatId),ct);var now=await gate.Now(ct);
        foreach(var s in seats.Where(s=>s.Status==SeatStatus.Held&&s.CurrentHoldId==hold.Id)) {s.CurrentHoldId=null;s.Status=SeatStatus.Available;}
        hold.Status=hold.ExpiresAtUtc<=now?HoldStatus.Expired:HoldStatus.Released;trip.Revision++;
        gate.Audit(actor,"HoldReleased",hold.Id.ToString(),now);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);await realtime.Publish(trip);
    }
    public async Task<Guid> Confirm(string actor,bool isStaff,ConfirmForm form,CancellationToken ct=default) {
        Guard.Input(form.HoldId!=Guid.Empty,"Thiếu mã giữ chỗ.");
        Guard.Input(form.ExchangeBookingId==null||isStaff,"Đổi vé cần nhân viên xử lý yêu cầu.");
        var targetTrip=await db.SeatHolds.Where(x=>x.Id==form.HoldId).Select(x=>(int?)x.TripId).SingleOrDefaultAsync(ct)??throw new BusinessException(404,"Không tìm thấy giữ chỗ.");
        var oldTrip=form.ExchangeBookingId.HasValue?await BookingTrip(form.ExchangeBookingId.Value,ct):targetTrip;
        await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted,ct);
        var trips=await LockTrips([targetTrip,oldTrip],ct);var trip=trips.Single(x=>x.Id==targetTrip);
        var hold=await Hold(form.HoldId,ct);Owner(hold,actor);
        var existing=await db.Bookings.SingleOrDefaultAsync(x=>x.SeatHoldId==hold.Id,ct);
        if(existing!=null) {
            Guard.Rule(existing.ReplacesBookingId==form.ExchangeBookingId,"Yêu cầu đổi vé khác với lần xác nhận trước.");
            Guard.Rule((!form.PickupStopId.HasValue||form.PickupStopId==existing.PickupStopId)&&
                (!form.DropoffStopId.HasValue||form.DropoffStopId==existing.DropoffStopId),"Vé đã được xác nhận với điểm đón/trả khác. Vui lòng kiểm tra thông tin trên vé.");
            await tx.CommitAsync(ct);return existing.Id;
        }
        var seats=await gate.Seats(targetTrip,hold.Items.Select(x=>x.TripSeatId),ct);
        Guard.Input(form.Passengers.Count==seats.Count&&SameSeats(form.Passengers.Select(x=>x.TripSeatId),seats.Select(x=>x.Id)),"Danh sách hành khách không khớp toàn bộ ghế đã giữ.");
        Booking? old=null;
        if(form.ExchangeBookingId.HasValue) {
            old=await LoadBooking(form.ExchangeBookingId.Value,ct);
            Guard.Rule(old.Status==BookingStatus.Active&&old.PaymentExpiresUtc==null,"Vé cũ chưa thanh toán hoặc không còn hiệu lực để đổi.");
            Guard.Rule(await db.ChangeRequests.AnyAsync(x=>x.BookingId==old.Id&&x.Kind==ChangeKind.Exchange&&x.Status==RequestStatus.Pending,ct),"Cần có yêu cầu đổi vé đang chờ xử lý.");
        }
        var bookingOwner=old?.OwnerId??actor;
        if(!string.IsNullOrWhiteSpace(form.CustomerEmail)) {
            Guard.Input(isStaff&&old==null,"Chỉ nhân viên bán hỗ trợ mới chọn tài khoản khách; đổi vé giữ nguyên chủ cũ.");
            var email=form.CustomerEmail.Trim().ToUpperInvariant();
            var customer=await db.Users.Where(u=>u.IsActive&&u.NormalizedEmail==email&&db.UserRoles.Any(ur=>ur.UserId==u.Id&&db.Roles.Any(r=>r.Id==ur.RoleId&&r.Name==Roles.Customer))).Select(u=>u.Id).SingleOrDefaultAsync(ct);
            Guard.Input(customer!=null,"Không tìm thấy tài khoản Khách hàng đang hoạt động với email này.");bookingOwner=customer!;
        }
        var now=await gate.Now(ct);DbGate.Sellable(trip,now);
        var originalTrip=old==null?null:trips.Single(x=>x.Id==oldTrip);
        Guard.Rule(hold.Status==HoldStatus.Active&&hold.ExpiresAtUtc>now,"Giữ chỗ đã hết hạn hoặc không còn hiệu lực.");
        Guard.Rule(seats.All(x=>x.Status==SeatStatus.Held&&x.CurrentHoldId==hold.Id),"Mã giữ chỗ đã cũ. Không thể đặt ghế hiện thuộc người khác.");
        var pair=await new TripStopService(db,gate).ResolveHeld(trip,hold,form.PickupStopId??hold.PickupStopId,form.DropoffStopId??hold.DropoffStopId,ct);
        if(old!=null) {
            Guard.Rule(IsBeforeDeparture(originalTrip!,now),"Chỉ hỗ trợ đổi trước khởi hành.");
            Guard.Rule(!await db.Bookings.AnyAsync(b=>b.ReplacesBookingId==old.Id&&b.Status==BookingStatus.Active&&
                b.PaymentExpiresUtc>now,ct),"Vé này đã có phương án đổi đang chờ thanh toán.");
            if(trip.Fare>0)
                Guard.Rule(await db.UserRoles.AnyAsync(ur=>ur.UserId==bookingOwner&&db.Roles.Any(r=>r.Id==ur.RoleId&&r.Name==Roles.Customer),ct),
                    "Vé mới cần được thanh toán bằng tài khoản khách hàng.");
        }
        var booking=new Booking{Id=Guid.NewGuid(),Code="VX"+Convert.ToHexString(RandomNumberGenerator.GetBytes(8)),OwnerId=bookingOwner,SeatHoldId=hold.Id,ContactName=form.ContactName.Trim(),ContactPhone=form.Phone.Trim(),CreatedUtc=now,Total=seats.Count*trip.Fare,Status=BookingStatus.Active,ReplacesBookingId=old?.Id};
        if(booking.Total>0&&(!isStaff||bookingOwner!=actor||old!=null)) {
            var deadline=now.AddSeconds(options.HoldSeconds);
            booking.PaymentExpiresUtc=new[]{deadline,trip.DepartureUtc,originalTrip?.DepartureUtc??deadline}.Min();
        }
        if(old!=null&&booking.PaymentExpiresUtc==null)
            await CompleteExchange(old,originalTrip!,actor,now,ct);
        booking.PickupStopId=pair.Pickup.Id;booking.DropoffStopId=pair.Dropoff.Id;
        booking.PickupName=pair.Pickup.Name;booking.DropoffName=pair.Dropoff.Name;
        booking.PickupAddress=pair.Pickup.Address;booking.DropoffAddress=pair.Dropoff.Address;
        booking.PickupAtUtc=TripStopService.AtUtc(trip,pair.Pickup);
        booking.DropoffAtUtc=TripStopService.AtUtc(trip,pair.Dropoff);
        booking.StopsIllustrative=pair.Pickup.IsIllustrative||pair.Dropoff.IsIllustrative;
        foreach(var seat in seats) {
            seat.Status=SeatStatus.Booked;seat.CurrentHoldId=null;
            booking.Tickets.Add(new Ticket{Id=Guid.NewGuid(),TripSeatId=seat.Id,Fare=trip.Fare,PassengerName=form.Passengers.Single(x=>x.TripSeatId==seat.Id).Name.Trim()});
        }
        db.Bookings.Add(booking);
        trip.Revision++;
        gate.Audit(actor,old==null?"BookingConfirmed":"ExchangeReplacementCreated",booking.Id.ToString(),now);
        await db.SaveChangesAsync(ct);
        // Final authoritative expiry check is part of the SQL state transition, AFTER any insert/index waits.
        var converted=await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE h SET [Status]=N'Confirmed' FROM dbo.SeatHolds h INNER JOIN dbo.Trips t ON t.Id=h.TripId WHERE h.Id={hold.Id} AND h.OwnerId={actor} AND h.[Status]=N'Active' AND h.ExpiresAtUtc>SYSUTCDATETIME() AND t.[Status]=N'Scheduled' AND t.SalesOpen=1 AND t.DepartureUtc>SYSUTCDATETIME()",ct);
        Guard.Rule(converted==1,"Giữ chỗ đã hết hạn tại thời điểm cập nhật. Giao dịch được hoàn tác toàn bộ.");
        if(originalTrip!=null)
            Guard.Rule(IsBeforeDeparture(originalTrip,await gate.Now(ct)),"Chuyến cũ đã đến giờ khởi hành.");
        hold.Status=HoldStatus.Confirmed;db.Entry(hold).State=EntityState.Unchanged;
        await tx.CommitAsync(ct);
        await realtime.Publish(trip);
        if(originalTrip!=null&&originalTrip.Id!=trip.Id&&booking.PaymentExpiresUtc==null)await realtime.Publish(originalTrip);
        return booking.Id;
    }
    public async Task<HoldView> UpdateStops(string actor,HoldStopsForm form,CancellationToken ct=default) {
        var tripId=await db.SeatHolds.Where(h=>h.Id==form.HoldId).Select(h=>(int?)h.TripId).SingleOrDefaultAsync(ct)
            ??throw new BusinessException(404,"Không tìm thấy giữ chỗ.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var trip=await gate.Trip(tripId,ct);var hold=await Hold(form.HoldId,ct);Owner(hold,actor);
        var now=await gate.Now(ct);DbGate.Sellable(trip,now);
        Guard.Rule(hold.Status==HoldStatus.Active&&hold.ExpiresAtUtc>now,"Lượt giữ đã hết hạn hoặc đã xác nhận.");
        var pair=await new TripStopService(db,gate).ResolveHeld(trip,hold,form.PickupStopId,form.DropoffStopId,ct);
        hold.PickupStopId=pair.Pickup.Id;hold.DropoffStopId=pair.Dropoff.Id;
        // No new hold, expiry extension or public personal-data event.
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return View(hold);
    }
    private async Task ResolvePending(Guid id,string actor,string note,DateTime now,CancellationToken ct) {
        foreach(var r in await db.ChangeRequests.Where(x=>x.BookingId==id&&x.Status==RequestStatus.Pending).ToListAsync(ct)) {r.Status=RequestStatus.Approved;r.ResolvedBy=actor;r.ResolvedUtc=now;r.Resolution=note;}
    }
    private async Task CompleteExchange(Booking original,Trip trip,string actor,DateTime now,CancellationToken ct) {
        var activeTickets=original.Tickets.Where(t=>t.IsActive).ToArray();
        await gate.Seats(trip.Id,activeTickets.Select(t=>t.TripSeatId),ct);
        CancelBooking(original);
        original.Status=BookingStatus.Exchanged;
        trip.Revision++;
        await ResolvePending(original.Id,actor,"Đã đổi vé. Khoản thanh toán của vé cũ chờ xử lý hoàn tiền.",now,ct);
        gate.Audit(actor,"BookingExchanged",original.Id.ToString(),now);
    }
    public static bool IsBeforeDeparture(Trip trip,DateTime now)=>
        trip.Status==TripStatus.Scheduled&&trip.DepartureUtc>now;

    public async Task Cancel(string actor,Guid id,CancellationToken ct=default) {
        var tripId=await BookingTrip(id,ct);
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var trip=await gate.Trip(tripId,ct);var b=await LoadBooking(id,ct);
        if(b.Status==BookingStatus.Cancelled) {await tx.CommitAsync(ct);return;}
        Guard.Rule(b.Status==BookingStatus.Active,"Vé đã đổi, không thể hủy lại.");
        await gate.Seats(tripId,b.Tickets.Select(x=>x.TripSeatId),ct);var now=await gate.Now(ct);
        Guard.Rule(IsBeforeDeparture(trip,now),"Chỉ hỗ trợ hủy vé trước khởi hành.");
        CancelBooking(b);await ResolvePending(id,actor,"Đã hủy; khoản đã thanh toán chờ xử lý hoàn tiền.",now,ct);
        trip.Revision++;gate.Audit(actor,"BookingCancelled",id.ToString(),now);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);await realtime.Publish(trip);
    }
    private static void CancelBooking(Booking b) {
        b.Status=BookingStatus.Cancelled;
        foreach(var ticket in b.Tickets.Where(x=>x.IsActive)) {ticket.IsActive=false;ticket.Seat.Status=SeatStatus.Available;ticket.Seat.CurrentHoldId=null;}
    }
    public async Task<int> SimulatePayment(string actor,Guid id,CancellationToken ct) {
        var target=Guard.Found(await db.Bookings.AsNoTracking().Where(b=>b.Id==id)
            .Select(b=>new{TripId=b.Hold.TripId,b.ReplacesBookingId}).SingleOrDefaultAsync(ct));
        var tripId=target.TripId;
        var oldTripId=target.ReplacesBookingId.HasValue?await BookingTrip(target.ReplacesBookingId.Value,ct):tripId;
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var trips=await LockTrips([tripId,oldTripId],ct);
        var trip=trips.Single(t=>t.Id==tripId);
        var booking=await LoadBooking(id,ct);
        if(booking.OwnerId!=actor)throw new BusinessException(403,"Bạn không có quyền thanh toán vé này.");
        const string expiredMessage="Đã hết hạn thanh toán. Vui lòng chọn ghế và đặt lại.";
        Guard.Rule(booking.Status!=BookingStatus.Expired,expiredMessage);
        Guard.Rule(booking.Status==BookingStatus.Active,"Vé đã hủy hoặc đổi, không thể hoàn tất thanh toán.");
        if(booking.Payments.Any(p=>p.Kind==MoneyKind.Receipt&&p.ActorId==actor&&p.RequestKey==id)||booking.Balance>=booking.Total) {
            await tx.CommitAsync(ct);
            return tripId;
        }
        var now=await gate.Now(ct);
        Guard.Rule(booking.PaymentExpiresUtc is not DateTime deadline||deadline>now,expiredMessage);
        var amount=booking.Total-booking.Balance;
        Guard.Rule(booking.Status==BookingStatus.Active&&IsBeforeDeparture(trip,now)&&amount>0,
            "Chỉ hoàn tất thanh toán cho vé còn hiệu lực trước khi chuyến khởi hành.");
        Trip? exchangedTrip=null;
        if(target.ReplacesBookingId is Guid originalId) {
            var original=await LoadBooking(originalId,ct);
            if(original.Status!=BookingStatus.Exchanged) {
                exchangedTrip=trips.Single(t=>t.Id==oldTripId);
                Guard.Rule(original.Status==BookingStatus.Active&&IsBeforeDeparture(exchangedTrip,now)&&
                    await db.ChangeRequests.AnyAsync(r=>r.BookingId==originalId&&r.Kind==ChangeKind.Exchange&&r.Status==RequestStatus.Pending,ct),
                    "Yêu cầu đổi vé không còn hiệu lực.");
                await CompleteExchange(original,exchangedTrip,actor,now,ct);
            }
        }
        booking.SimulatedPaidUtc=now;
        booking.SimulatedPaidAmount=amount;
        db.Payments.Add(new PaymentEntry {
            Id=Guid.NewGuid(),BookingId=id,ActorId=actor,RequestKey=id,
            Amount=amount,Kind=MoneyKind.Receipt,CreatedUtc=now,Note="Thanh toán QR (đồ án)"
        });
        gate.Audit(actor,"QrPaymentCompleted",id.ToString(),now);
        if(booking.PaymentExpiresUtc!=null)trip.Revision++;
        await db.SaveChangesAsync(ct);
        if(booking.PaymentExpiresUtc!=null)await ClosePaymentWindow(booking,true,expiredMessage,ct);
        await tx.CommitAsync(ct);
        if(booking.PaymentExpiresUtc!=null)await realtime.Publish(trip);
        if(exchangedTrip!=null&&exchangedTrip.Id!=tripId)await realtime.Publish(exchangedTrip);
        return tripId;
    }
    private async Task ClosePaymentWindow(Booking booking,bool settled,string expiredMessage,CancellationToken ct) {
        // Evaluate the deadline in SQL after receipt writes, which may have waited on locks.
        var updated=await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE dbo.Bookings SET PaymentExpiresUtc={ (settled?null:booking.PaymentExpiresUtc) } WHERE Id={booking.Id} AND PaymentExpiresUtc>SYSUTCDATETIME()",ct);
        Guard.Rule(updated==1,expiredMessage);
    }
    public async Task Money(string actor,PaymentForm form,CancellationToken ct=default) {
        Guard.Input(form.RequestKey!=Guid.Empty,"Thiếu mã thao tác thu/hoàn.");var tripId=await BookingTrip(form.BookingId,ct);
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var trip=await gate.Trip(tripId,ct);var b=await LoadBooking(form.BookingId,ct);
        var previous=await db.Payments.SingleOrDefaultAsync(x=>x.ActorId==actor&&x.RequestKey==form.RequestKey,ct);
        if(previous!=null) {
            Guard.Rule(previous.BookingId==b.Id&&(previous.Kind==MoneyKind.Refund)==form.Refund
                &&(form.Amount is null||Math.Abs(previous.Amount)==form.Amount),"Mã thao tác đã dùng cho khoản khác.");
            await tx.CommitAsync(ct);return;
        }
        var balance=b.Payments.Sum(x=>x.Amount);
        var remaining=form.Refund?balance:b.Total-balance;
        if(form.Refund)Guard.Rule(b.Status!=BookingStatus.Active&&remaining>0,"Chỉ hoàn khoản đã thu của vé hủy/đổi.");
        else Guard.Rule(b.Status==BookingStatus.Active&&remaining>0&&b.ReplacesBookingId==null,
            "Vé đã thu đủ, không còn hiệu lực hoặc cần hoàn tất đổi vé bằng QR.");
        var requested=form.Amount is long value?(decimal)value:remaining;
        Guard.Input(requested>0,"Số tiền phải lớn hơn 0.");
        Guard.Rule(requested<=remaining,"Số tiền vượt khoản còn phải thu hoặc hoàn. Vui lòng tải lại vé để xem số dư.");
        var amount=form.Refund?-requested:requested;
        var now=await gate.Now(ct);
        const string expiredMessage="Đã hết hạn thanh toán.";
        var timedReceipt=!form.Refund&&b.PaymentExpiresUtc!=null;
        Guard.Rule(!timedReceipt||b.PaymentExpiresUtc>now,expiredMessage);
        var settled=requested==remaining;
        db.Payments.Add(new PaymentEntry{Id=Guid.NewGuid(),BookingId=b.Id,ActorId=actor,RequestKey=form.RequestKey,Amount=amount,Kind=form.Refund?MoneyKind.Refund:MoneyKind.Receipt,CreatedUtc=now,Note=form.Note?.Trim()??""});
        gate.Audit(actor,form.Refund?"RefundRecorded":"ReceiptRecorded",b.Id.ToString(),now);
        if(timedReceipt&&settled)trip.Revision++;
        await db.SaveChangesAsync(ct);
        if(timedReceipt)await ClosePaymentWindow(b,settled,expiredMessage,ct);
        await tx.CommitAsync(ct);
        if(timedReceipt&&settled)await realtime.Publish(trip);
    }
    public async Task RequestChange(string actor,RequestForm f,CancellationToken ct=default) {
        var tripId=await BookingTrip(f.BookingId,ct);await using var tx=await db.Database.BeginTransactionAsync(ct);
        var trip=await gate.Trip(tripId,ct);var b=await LoadBooking(f.BookingId,ct);var now=await gate.Now(ct);
        if(b.OwnerId!=actor)throw new BusinessException(403,"Bạn không có quyền gửi yêu cầu cho vé này.");
        Guard.Rule(b.Status==BookingStatus.Active&&b.PaymentExpiresUtc==null&&IsBeforeDeparture(trip,now),"Yêu cầu chỉ áp dụng cho vé còn hiệu lực trước khởi hành. Vui lòng hoàn tất thanh toán QR nếu vé đang chờ.");
        var previous=await db.ChangeRequests.SingleOrDefaultAsync(x=>x.BookingId==b.Id&&x.Status==RequestStatus.Pending,ct);
        if(previous!=null) {Guard.Rule(previous.Kind==f.Kind,"Đã có yêu cầu khác đang chờ.");await tx.CommitAsync(ct);return;}
        db.ChangeRequests.Add(new ChangeRequest{Id=Guid.NewGuid(),BookingId=b.Id,Kind=f.Kind,Status=RequestStatus.Pending,Reason=f.Reason.Trim(),CreatedUtc=now});
        gate.Audit(actor,"ChangeRequested",b.Id.ToString(),now);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    public async Task RejectRequest(string actor,Guid requestId,string resolution,CancellationToken ct=default) {
        Guard.Input(!string.IsNullOrWhiteSpace(resolution)&&resolution.Length<=500,"Nhập lý do từ chối (tối đa 500 ký tự).");
        var tripId=await db.ChangeRequests.Where(x=>x.Id==requestId).Select(x=>(int?)x.Booking.Hold.TripId).SingleOrDefaultAsync(ct)??throw new BusinessException(404,"Không có yêu cầu.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);await gate.Trip(tripId,ct);
        var r=Guard.Found(await db.ChangeRequests.SingleOrDefaultAsync(x=>x.Id==requestId,ct));
        Guard.Rule(r.Status==RequestStatus.Pending,"Yêu cầu đã xử lý.");r.Status=RequestStatus.Rejected;r.Resolution=resolution.Trim();r.ResolvedBy=actor;r.ResolvedUtc=await gate.Now(ct);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    public async Task<int> ExpireTrip(int tripId,CancellationToken ct=default) {
        await using var tx=await db.Database.BeginTransactionAsync(ct);var trip=await gate.Trip(tripId,ct);var now=await gate.Now(ct);
        var count=await ExpireUnderGate(trip,now,ct);
        count+=await ExpirePaymentsUnderGate(trip,now,ct);
        if(count>0){trip.Revision++;await db.SaveChangesAsync(ct);}
        await tx.CommitAsync(ct);if(count>0)await realtime.Publish(trip);return count;
    }
    public async Task SetTripState(string actor,bool isAdmin,int id,TripStatus? next,bool? sales,CancellationToken ct=default) {
        await using var tx=await db.Database.BeginTransactionAsync(ct);var trip=await gate.Trip(id,ct);
        if(!isAdmin&&trip.DriverId!=actor&&trip.CoDriverId!=actor)throw new BusinessException(403,"Chuyến này không được phân công cho bạn.");
        var now=await gate.Now(ct);
        if(sales.HasValue) {
            Guard.Rule(isAdmin&&trip.Status==TripStatus.Scheduled&&trip.DepartureUtc>now,"Không thể mở/đóng bán chuyến này.");trip.SalesOpen=sales.Value;
        }
        if(next.HasValue) {
            Guard.Input(Enum.IsDefined(next.Value),"Trạng thái không hợp lệ.");
            var valid=(trip.Status,next.Value) is (TripStatus.Scheduled,TripStatus.Boarding) or (TripStatus.Boarding,TripStatus.InProgress) or (TripStatus.InProgress,TripStatus.Completed);
            var cancel=isAdmin&&next==TripStatus.Cancelled&&trip.Status is TripStatus.Scheduled or TripStatus.Boarding;
            Guard.Rule(valid||cancel,"Không đúng thứ tự chuẩn bị chạy → đang chạy → hoàn thành, hoặc chuyến không thể hủy.");
            trip.Status=next.Value;trip.SalesOpen=false;
            if(cancel) {
                var bookings=await db.Bookings.Include(x=>x.Tickets).ThenInclude(x=>x.Seat).Where(x=>x.Hold.TripId==id&&x.Status==BookingStatus.Active).ToListAsync(ct);
                await gate.Seats(id,bookings.SelectMany(x=>x.Tickets).Select(x=>x.TripSeatId),ct);
                foreach(var b in bookings) {CancelBooking(b);await ResolvePending(b.Id,actor,"Chuyến bị hủy; khoản đã thanh toán chờ xử lý hoàn tiền.",now,ct);}
            }
        }
        if(!trip.SalesOpen) {
            var holds=await db.SeatHolds.Where(x=>x.TripId==id&&x.Status==HoldStatus.Active).ToListAsync(ct);
            var held=await db.TripSeats.Where(x=>x.TripId==id&&x.Status==SeatStatus.Held).Select(x=>x.Id).ToListAsync(ct);
            foreach(var s in await gate.Seats(id,held,ct)) {s.Status=SeatStatus.Available;s.CurrentHoldId=null;}
            foreach(var h in holds)h.Status=h.ExpiresAtUtc<=now?HoldStatus.Expired:HoldStatus.Released;
        }
        trip.Revision++;gate.Audit(actor,"TripStateChanged",id.ToString(),now,$"{trip.Status}; SalesOpen={trip.SalesOpen}");
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);await realtime.Publish(trip);
    }
}
