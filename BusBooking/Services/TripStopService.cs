using BusBooking.Data;
using BusBooking.Models;
using BusBooking.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Services;

public sealed class TripStopService(AppDbContext db,DbGate gate) {
    public static int Duration(Trip trip) => checked((int)(trip.ArrivalUtc-trip.DepartureUtc).TotalMinutes);
    public static DateTime AtUtc(Trip trip,TripStop stop)=>BusinessTime.Utc((stop.StopOrder==1000||stop.StopOrder>1000&&stop.OffsetMinutes==Duration(trip))?trip.ArrivalUtc:trip.DepartureUtc.AddMinutes(stop.OffsetMinutes));
    public static StopView ToView(Trip trip,TripStop stop) => new(stop.Id,stop.StopOrder,StopPresentation.Name(stop.Name),
        StopPresentation.Address(stop.Address),AtUtc(trip,stop),
        stop.CanPickup,stop.CanDropoff,stop.IsIllustrative);

    public static (TripStop Pickup,TripStop Dropoff) Select(Trip trip,IReadOnlyList<TripStop> stops,int? pickup,int? dropoff) {
        var enabled=stops.Where(s=>s.TripId==trip.Id&&s.IsActive).OrderBy(s=>s.StopOrder).ToList();
        var p=pickup.HasValue?enabled.SingleOrDefault(s=>s.Id==pickup):enabled.FirstOrDefault(s=>s.CanPickup);
        var d=dropoff.HasValue?enabled.SingleOrDefault(s=>s.Id==dropoff):enabled.LastOrDefault(s=>s.CanDropoff);
        if(p==null||d==null)throw new BusinessException(400,"Chọn điểm đón/trả đang hoạt động của đúng chuyến.");
        Guard.Input(p.CanPickup&&d.CanDropoff&&p.StopOrder<d.StopOrder&&AtUtc(trip,p)<AtUtc(trip,d),
            "Điểm trả phải sau điểm đón, không trùng hoặc ngược hành trình.");
        Guard.Input(p.OffsetMinutes>=0&&d.OffsetMinutes<=Duration(trip),"Giờ đón/trả không nằm trong lịch chuyến.");
        return (p,d);
    }
    public async Task<(TripStop Pickup,TripStop Dropoff)> Resolve(Trip trip,int? pickup,int? dropoff,CancellationToken ct) =>
        Select(trip,await db.TripStops.AsNoTracking().Where(s=>s.TripId==trip.Id).ToListAsync(ct),pickup,dropoff);

    public async Task<(TripStop Pickup,TripStop Dropoff)> ResolveNew(Trip trip,int? pickup,int? dropoff,CancellationToken ct=default) {
        var route=await db.Routes.AsNoTracking().SingleAsync(x=>x.Id==trip.BusRouteId,ct);
        var rows=await db.TripStops.AsNoTracking().Where(x=>x.TripId==trip.Id).ToListAsync(ct);
        return StopChoicePolicy.SelectNew(trip,route,rows,pickup,dropoff);
    }
    public async Task<(TripStop Pickup,TripStop Dropoff)> ResolveHeld(Trip trip,SeatHold hold,int? pickup,int? dropoff,CancellationToken ct=default) {
        var route=await db.Routes.AsNoTracking().SingleAsync(x=>x.Id==trip.BusRouteId,ct);
        var rows=await db.TripStops.AsNoTracking().Where(x=>x.TripId==trip.Id).ToListAsync(ct);
        return StopChoicePolicy.SelectHeld(trip,route,rows,hold,pickup,dropoff);
    }

    // Caller owns the catalog/trip gate and transaction. Never edits an existing custom schedule.
    public static async Task EnsureEndpoints(AppDbContext db,Trip trip,BusRoute route,CancellationToken ct=default) {
        var stops=await db.TripStops.Where(s=>s.TripId==trip.Id).OrderBy(s=>s.StopOrder).ToListAsync(ct);
        if(stops.Count==0) {
            db.TripStops.AddRange(new TripStop{TripId=trip.Id,StopOrder=0,Name="Điểm đầu · "+route.Origin,CanPickup=true},
                new TripStop{TripId=trip.Id,StopOrder=1000,Name="Điểm cuối · "+route.Destination,OffsetMinutes=Duration(trip),CanDropoff=true});
        } else if(stops.Count==2&&stops[0].StopOrder==0&&stops[1].StopOrder==1000&&
            stops.All(s=>s.Name.StartsWith("Điểm đầu · ")||s.Name.StartsWith("Điểm cuối · "))) {
            stops[0].Name="Điểm đầu · "+route.Origin;stops[1].Name="Điểm cuối · "+route.Destination;
            stops[1].OffsetMinutes=Duration(trip);
        } else {
            Guard.Rule(stops[0].OffsetMinutes==0&&stops[^1].OffsetMinutes==Duration(trip),
                "Lịch điểm dừng không khớp giờ mới. Giữ lịch chuyến và cấu hình điểm dừng trước khi đổi thời gian.");
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task<TripStopsForm> Edit(int tripId,CancellationToken ct=default) {
        var trip=Guard.Found(await db.Trips.AsNoTracking().Include(t=>t.Route).SingleOrDefaultAsync(t=>t.Id==tripId,ct));
        var rows=await db.TripStops.AsNoTracking().Where(s=>s.TripId==tripId).OrderBy(s=>s.StopOrder).ToListAsync(ct);
        return new TripStopsForm{TripId=trip.Id,Revision=trip.Revision,RouteLabel=trip.Route.Label,
            DepartureUtc=trip.DepartureUtc,ArrivalUtc=trip.ArrivalUtc,
            Locked=trip.Status!=TripStatus.Scheduled||trip.DepartureUtc<=await gate.Now(ct)||await db.SeatHolds.AnyAsync(h=>h.TripId==tripId,ct),
            Stops=rows.Select(s=>new StopEditRow{Id=s.Id,Name=StopPresentation.Name(s.Name),Address=StopPresentation.Address(s.Address),OffsetMinutes=s.OffsetMinutes,
                CanPickup=s.CanPickup,CanDropoff=s.CanDropoff,IsActive=s.IsActive,IsIllustrative=s.IsIllustrative}).ToList()};
    }
    public async Task Save(string actor,TripStopsForm form,CancellationToken ct=default) {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var trip=await gate.Trip(form.TripId,ct);
        Guard.Rule(trip.Revision==form.Revision,"Chuyến vừa được cập nhật. Tải lại trước khi sửa lịch điểm.");
        Guard.Rule(trip.Status==TripStatus.Scheduled&&trip.DepartureUtc>await gate.Now(ct)&&!await db.SeatHolds.AnyAsync(h=>h.TripId==trip.Id,ct),
            "Chuyến đã phát sinh giữ/vé hoặc đã chạy: khóa lịch điểm để bảo toàn lựa chọn. Không xóa hold/vé để mở khóa.");
        Validate(form.Stops,Duration(trip));
        var current=await db.TripStops.Where(s=>s.TripId==trip.Id).ToListAsync(ct);
        var ids=form.Stops.Where(s=>s.Id!=0).Select(s=>s.Id).ToList();
        Guard.Input(ids.Distinct().Count()==ids.Count&&ids.All(id=>current.Any(s=>s.Id==id)),"Điểm dừng không thuộc chuyến này.");
        // No holds exist under the shared trip gate. Removing unreferenced rows is safe.
        db.TripStops.RemoveRange(current.Where(s=>!ids.Contains(s.Id)));
        for(int i=0;i<form.Stops.Count;i++) {
            var f=form.Stops[i];var row=f.Id==0?new TripStop{TripId=trip.Id}:current.Single(s=>s.Id==f.Id);
            // Temporary negative order avoids unique-index swaps in one SaveChanges.
            if(f.Id!=0)row.StopOrder=-i-1;
        }
        await db.SaveChangesAsync(ct);
        for(int i=0;i<form.Stops.Count;i++) {
            var f=form.Stops[i];var row=f.Id==0?new TripStop{TripId=trip.Id}:current.Single(s=>s.Id==f.Id);
            row.StopOrder=i==form.Stops.Count-1?1000:i*10;
            var enteredName=f.Name.Trim();var enteredAddress=string.IsNullOrWhiteSpace(f.Address)?null:f.Address.Trim();
            // A clean label posted unchanged is presentation, not a request to rewrite provenance.
            row.Name=f.Id!=0&&enteredName==StopPresentation.Name(row.Name)?row.Name:enteredName;
            row.Address=f.Id!=0&&enteredAddress==StopPresentation.Address(row.Address)?row.Address:enteredAddress;
            row.OffsetMinutes=f.OffsetMinutes;row.CanPickup=f.CanPickup;row.CanDropoff=f.CanDropoff;
            row.IsActive=f.IsActive;row.IsIllustrative=f.IsIllustrative;
            if(f.Id==0)db.TripStops.Add(row);
        }
        trip.Revision++;gate.Audit(actor,"TripStopsSaved",trip.Id.ToString(),await gate.Now(ct));
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    public static void Validate(IReadOnlyList<StopEditRow> rows,int duration) {
        Guard.Input(rows.Count>=2&&rows.Count<=20,"Lịch cần từ 2 đến 20 điểm.");
        Guard.Input(rows[0].OffsetMinutes==0&&rows[^1].OffsetMinutes==duration&&rows[0].IsActive&&rows[^1].IsActive&&
            rows[0].CanPickup&&rows[^1].CanDropoff,"Điểm đầu/cuối phải hoạt động và khớp giờ đi/đến toàn chuyến.");
        for(int i=0;i<rows.Count;i++) {
            var row=rows[i];
            Guard.Input(!string.IsNullOrWhiteSpace(row.Name)&&row.Name.Trim().Length<=160&&(row.Address?.Length??0)<=500,
                "Tên điểm bắt buộc, tối đa 160 ký tự; địa chỉ tối đa 500 ký tự.");
            Guard.Input(row.OffsetMinutes>=0&&row.OffsetMinutes<=duration&&(i==0||row.OffsetMinutes>=rows[i-1].OffsetMinutes),
                "Thời gian điểm dừng phải theo thứ tự và nằm trong lịch chuyến.");
        }
    }
}
