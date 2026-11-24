using BusBooking.Data;
using BusBooking.Models;
using BusBooking.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace BusBooking.Services;
public class CatalogService(AppDbContext db,DbGate gate,RealtimePublisher realtime,UserManager<AppUser> users,IOptions<BookingOptions> config) {
    public async Task SaveRoute(string actor,RouteForm f,CancellationToken ct=default) {
        Guard.Input(f.Origin.Trim()!=f.Destination.Trim(),"Điểm đi phải khác điểm đến.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);await gate.Catalog(ct);
        var route=f.Id==0?new BusRoute():Guard.Found(await db.Routes.FindAsync([f.Id],ct));
        if(f.Id!=0)Guard.Rule(!await db.Trips.AnyAsync(x=>x.BusRouteId==f.Id,ct)||route.Origin==f.Origin.Trim()&&route.Destination==f.Destination.Trim()&&route.DurationMinutes==f.DurationMinutes,"Tuyến đã có chuyến: chỉ sửa mô tả/trạng thái; tạo tuyến mới để đổi hành trình.");
        route.Origin=f.Origin.Trim();route.Destination=f.Destination.Trim();route.Description=f.Description.Trim();route.DurationMinutes=f.DurationMinutes;route.IsActive=f.IsActive;
        if(f.Id==0)db.Routes.Add(route);gate.Audit(actor,"RouteSaved",f.Id.ToString(),await gate.Now(ct));await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    public async Task SaveLayout(string actor,LayoutForm f,CancellationToken ct=default) {
        await using var tx=await db.Database.BeginTransactionAsync(ct);await gate.Catalog(ct);
        var layout=f.Id==0?new SeatLayout():Guard.Found(await db.Layouts.Include(x=>x.Seats).SingleOrDefaultAsync(x=>x.Id==f.Id,ct));
        bool shape=layout.Rows!=f.Rows||layout.Columns!=f.Columns||layout.Floors!=f.Floors;
        if(f.Id!=0&&shape)Guard.Rule(!await db.Vehicles.AnyAsync(x=>x.SeatLayoutId==f.Id,ct),"Sơ đồ đã gắn xe: không đổi cấu trúc; hãy tạo sơ đồ mới.");
        layout.Name=f.Name.Trim();layout.IsActive=f.IsActive;
        if(shape||f.Id==0) {
            if(f.Id!=0) {db.PhysicalSeats.RemoveRange(layout.Seats);await db.SaveChangesAsync(ct);layout.Seats.Clear();}
            layout.Rows=f.Rows;layout.Columns=f.Columns;layout.Floors=f.Floors;
            for(int floor=1;floor<=f.Floors;floor++)for(int row=1;row<=f.Rows;row++)for(int col=1;col<=f.Columns;col++)
                layout.Seats.Add(new PhysicalSeat{Code=$"{(char)('A'+floor-1)}{(row-1)*f.Columns+col:00}",Row=row,Column=col,Floor=floor});
        }
        if(f.Id==0)db.Layouts.Add(layout);gate.Audit(actor,"LayoutSaved",f.Id.ToString(),await gate.Now(ct));await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    public async Task SaveVehicle(string actor,VehicleForm f,CancellationToken ct=default) {
        await using var tx=await db.Database.BeginTransactionAsync(ct);await gate.Catalog(ct);
        var layout=Guard.Found(await db.Layouts.FindAsync([f.SeatLayoutId],ct));
        var vehicle=f.Id==0?new Vehicle():Guard.Found(await db.Vehicles.FindAsync([f.Id],ct));
        Guard.Rule(layout.IsActive||vehicle.SeatLayoutId==layout.Id,"Sơ đồ đang ngừng dùng.");
        if(f.Id!=0)Guard.Rule(!await db.Trips.AnyAsync(x=>x.VehicleId==f.Id,ct)||vehicle.SeatLayoutId==f.SeatLayoutId,"Xe đã có chuyến, không thay sơ đồ ghế.");
        var plate=f.Plate.Trim().ToUpperInvariant();
        Guard.Input(!await db.Vehicles.AnyAsync(x=>x.Plate==plate&&x.Id!=f.Id,ct),"Biển số này đã được sử dụng.");
        vehicle.Name=f.Name.Trim();
        vehicle.Plate=plate;
        vehicle.Kind=f.Kind??(f.Id==0||vehicle.SeatLayoutId!=layout.Id
            ? (layout.Floors>1||layout.Name.Contains("giường nằm",StringComparison.OrdinalIgnoreCase)
                ? VehicleKind.Sleeper : VehicleKind.Seated)
            : vehicle.Kind);
        vehicle.SeatLayoutId=f.SeatLayoutId;
        vehicle.IsActive=f.IsActive;
        if(f.Id==0)db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(ct);
        gate.Audit(actor,"VehicleSaved",vehicle.Id.ToString(),await gate.Now(ct));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
    public async Task SaveDriver(string actor,DriverForm f,CancellationToken ct=default) {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        await gate.Catalog(ct);
        var now=await gate.Now(ct);
        Guard.Input(f.DateOfBirth is null||f.DateOfBirth<=DateOnly.FromDateTime(BusinessTime.Vietnam(now)),
            "Ngày sinh phải là hôm nay hoặc một ngày trước đó.");

        var creating=string.IsNullOrEmpty(f.Id);
        var user=creating?new AppUser():Guard.Found(await users.FindByIdAsync(f.Id!));
        if(!creating) {
            Guard.Rule(await users.IsInRoleAsync(user,Roles.Driver),"Tài khoản này không có vai trò tài xế.");
            Guard.Rule(actor!=user.Id||f.IsActive,"Không thể tự ngừng hoạt động tài khoản đang đăng nhập.");
            if(user.IsActive&&!f.IsActive) await EnsureUnassigned(user.Id,ct);
        }

        var email=f.Email.Trim();
        var phone=string.IsNullOrWhiteSpace(f.PhoneNumber)?null:f.PhoneNumber.Trim();
        var emailChanged=!string.Equals(user.Email,email,StringComparison.OrdinalIgnoreCase);
        var revokeSession=emailChanged||user.IsActive!=f.IsActive;
        if(emailChanged)user.EmailConfirmed=false;
        if(user.PhoneNumber!=phone)user.PhoneNumberConfirmed=false;
        user.FullName=f.FullName.Trim();
        user.Email=email;
        if(creating)user.UserName=email;
        user.PhoneNumber=phone;
        user.DateOfBirth=f.DateOfBirth;
        user.IsActive=f.IsActive;

        if(creating) {
            Guard.Input(!string.IsNullOrWhiteSpace(f.Password),"Vui lòng nhập mật khẩu ban đầu.");
            Check(await users.CreateAsync(user,f.Password!));
            Check(await users.AddToRoleAsync(user,Roles.Driver));
        }else if(revokeSession) {
            Check(await users.UpdateSecurityStampAsync(user));
        }else {
            Check(await users.UpdateAsync(user));
        }
        gate.Audit(actor,"DriverSaved",user.Id,now);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
    public async Task ResetDriverPassword(string actor,string driverId,string newPassword,CancellationToken ct) {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        await gate.Catalog(ct);
        var driver=Guard.Found(await users.FindByIdAsync(driverId));
        Guard.Rule(await users.IsInRoleAsync(driver,Roles.Driver)&&!await users.IsInRoleAsync(driver,Roles.Admin),
            "Chỉ hỗ trợ đặt mật khẩu cho tài khoản tài xế.");
        var hadPassword=await users.HasPasswordAsync(driver);
        var token=await users.GeneratePasswordResetTokenAsync(driver);
        Check(await users.ResetPasswordAsync(driver,token,newPassword));
        gate.Audit(actor,hadPassword?"DriverPasswordReset":"DriverPasswordSet",driver.Id,await gate.Now(ct));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
    private async Task Assigned(string? userId,string role) {
        if(userId==null)return;var user=Guard.Found(await users.FindByIdAsync(userId));Guard.Rule(user.IsActive&&await users.IsInRoleAsync(user,role),"Tài khoản phân công không hoạt động hoặc không đúng vai trò.");
    }
    public async Task SaveTrip(string actor,TripForm f,CancellationToken ct=default,int? copyStopsFrom=null) {
        await using var tx=await db.Database.BeginTransactionAsync(ct);await gate.Catalog(ct);
        var trip=await SaveTripUnderGate(actor,f,ct,copyStopsFrom);
        await tx.CommitAsync(ct);await realtime.Publish(trip);
    }

    public async Task<IReadOnlyList<TripBatchRow>> BatchTrips(string actor,TripBatchForm form,bool create,CancellationToken ct) {
        var from=form.From.Date;
        var to=form.To.Date;
        Guard.Input(from.Year>=1900&&to.Year<=9998&&to>=from&&(to-from).Days<93,
            "Chọn khoảng ngày hợp lệ, tối đa 93 ngày mỗi lần.");
        Guard.Input(form.DepartureTime.HasValue&&form.DriverId!=form.CoDriverId,"Chọn giờ khởi hành và hai tài xế khác nhau.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        if(create)await gate.Catalog(ct);
        var now=await gate.Now(ct);
        var route=Guard.Found(await db.Routes.AsNoTracking().SingleOrDefaultAsync(r=>r.Id==form.BusRouteId,ct));
        var vehicle=Guard.Found(await db.Vehicles.AsNoTracking().Include(v=>v.Layout).SingleOrDefaultAsync(v=>v.Id==form.VehicleId,ct));
        Guard.Rule(route.IsActive&&vehicle.IsActive&&vehicle.Layout.IsActive,"Tuyến, xe hoặc sơ đồ ghế đang ngừng hoạt động.");
        await Assigned(form.DriverId,Roles.Driver);
        await Assigned(form.CoDriverId,Roles.Driver);
        await Assigned(form.StaffId,Roles.Staff);
        var drivers=new[]{form.DriverId,form.CoDriverId};
        var buffer=config.Value.ScheduleBufferMinutes;
        var first=BusinessTime.FromVietnam(from.Add(form.DepartureTime!.Value.ToTimeSpan()));
        var last=first.AddDays((to-from).Days).AddMinutes(route.DurationMinutes);
        var lower=first.AddMinutes(-buffer);
        var upper=last.AddMinutes(buffer);
        var occupied=await db.Trips.AsNoTracking().Where(t=>t.Status!=TripStatus.Cancelled&&t.DepartureUtc<upper&&t.ArrivalUtc>lower&&
            (t.VehicleId==form.VehicleId||drivers.Contains(t.DriverId!)||drivers.Contains(t.CoDriverId!)||
                form.StaffId!=null&&t.StaffId==form.StaffId)).ToListAsync(ct);
        var rows=new List<TripBatchRow>();
        var created=new List<Trip>();
        var accepted=new List<(DateTime Start,DateTime End)>();
        for(var day=from;day<=to;day=day.AddDays(1)) {
            var departure=day.Add(form.DepartureTime.Value.ToTimeSpan());
            var arrival=departure.AddMinutes(route.DurationMinutes);
            var start=BusinessTime.FromVietnam(departure);
            var end=BusinessTime.FromVietnam(arrival);
            var duplicate=occupied.FirstOrDefault(t=>t.BusRouteId==form.BusRouteId&&t.VehicleId==form.VehicleId&&t.DepartureUtc==start);
            var overlaps=occupied.Where(t=>t.DepartureUtc<end.AddMinutes(buffer)&&t.ArrivalUtc>start.AddMinutes(-buffer)).ToList();
            var status=duplicate!=null?"Đã có chuyến":start<=now?"Đã qua giờ khởi hành":
                overlaps.Any(t=>t.VehicleId==form.VehicleId)?"Trùng lịch xe":
                overlaps.Any(t=>drivers.Contains(t.DriverId)||drivers.Contains(t.CoDriverId))?"Trùng lịch tài xế":
                overlaps.Any(t=>form.StaffId!=null&&t.StaffId==form.StaffId)?"Trùng lịch nhân viên":
                accepted.Any(t=>t.Start<end.AddMinutes(buffer)&&t.End>start.AddMinutes(-buffer))?"Trùng lịch trong đợt tạo":
                "Có thể tạo";
            var row=new TripBatchRow(departure,arrival,status,duplicate?.Id);
            if(row.Ready) {
                accepted.Add((start,end));
                if(create) {
                    var trip=await SaveTripUnderGate(actor,new TripForm {
                        BusRouteId=form.BusRouteId,VehicleId=form.VehicleId,DepartureLocal=departure,ArrivalLocal=arrival,
                        Fare=form.Fare,DriverId=form.DriverId,CoDriverId=form.CoDriverId,StaffId=form.StaffId
                    },ct);
                    created.Add(trip);
                    row=row with {Status="Đã tạo",TripId=trip.Id};
                }
            }
            rows.Add(row);
        }
        await tx.CommitAsync(ct);
        foreach(var trip in created)await realtime.Publish(trip);
        return rows;
    }

    private async Task<Trip> SaveTripUnderGate(string actor,TripForm f,CancellationToken ct,int? copyStopsFrom=null) {
        Trip trip=f.Id==0?new Trip():await gate.Trip(f.Id,ct);
        var start=BusinessTime.FromVietnam(f.DepartureLocal);var end=BusinessTime.FromVietnam(f.ArrivalLocal);var now=await gate.Now(ct);
        Guard.Input(start>now&&end>start,"Khởi hành phải ở tương lai và giờ đến phải sau giờ đi.");
        var route=Guard.Found(await db.Routes.FindAsync([f.BusRouteId],ct));
        var vehicle=Guard.Found(await db.Vehicles.Include(x=>x.Layout).ThenInclude(x=>x.Seats).SingleOrDefaultAsync(x=>x.Id==f.VehicleId,ct));
        Guard.Rule(route.IsActive&&vehicle.IsActive&&vehicle.Layout.IsActive,"Tuyến, xe hoặc sơ đồ ghế đang ngừng hoạt động.");
        Guard.Rule(trip.Status==TripStatus.Scheduled,"Chỉ sửa chuyến chưa chạy.");
        var structural=f.Id!=0&&(trip.BusRouteId!=f.BusRouteId||trip.VehicleId!=f.VehicleId||trip.DepartureUtc!=start||trip.ArrivalUtc!=end||trip.Fare!=f.Fare);
        if(structural)Guard.Rule(!await db.SeatHolds.AnyAsync(x=>x.TripId==f.Id,ct),"Chuyến đã phát sinh giữ/đặt: khóa hành trình, xe, giờ và giá. Vẫn có thể đổi phân công hợp lệ.");
        if(f.Id!=0&&trip.BusRouteId!=f.BusRouteId) {
            var oldRoute=await db.Routes.SingleAsync(r=>r.Id==trip.BusRouteId,ct);
            var stopRows=await db.TripStops.Where(s=>s.TripId==trip.Id).OrderBy(s=>s.StopOrder).ToListAsync(ct);
            Guard.Rule(stopRows.Count==0||(stopRows.Count==2&&stopRows[0].Name=="Điểm đầu · "+oldRoute.Origin&&
                stopRows[1].Name=="Điểm cuối · "+oldRoute.Destination&&stopRows.All(s=>s.Address==null&&s.IsIllustrative)),
                "Chuyến có lịch điểm riêng: không tự mang các điểm sang tuyến khác. Giữ tuyến hiện tại hoặc tạo chuyến phù hợp.");
        }
        f.DriverId=string.IsNullOrWhiteSpace(f.DriverId)?null:f.DriverId;f.StaffId=string.IsNullOrWhiteSpace(f.StaffId)?null:f.StaffId;
        f.CoDriverId=string.IsNullOrWhiteSpace(f.CoDriverId)?null:f.CoDriverId;
        Guard.Input(f.CoDriverId==null||f.DriverId!=null&&f.DriverId!=f.CoDriverId,"Chọn hai tài xế khác nhau và phân công tài xế chính trước.");
        await Assigned(f.DriverId,Roles.Driver);await Assigned(f.CoDriverId,Roles.Driver);await Assigned(f.StaffId,Roles.Staff);
        var buffer=config.Value.ScheduleBufferMinutes;var lower=start.AddMinutes(-buffer);var upper=end.AddMinutes(buffer);
        var others=db.Trips.Where(x=>x.Id!=f.Id&&x.Status!=TripStatus.Cancelled&&x.DepartureUtc<upper&&x.ArrivalUtc>lower);
        Guard.Rule(!await others.AnyAsync(x=>x.VehicleId==f.VehicleId,ct),"Xe bị trùng lịch. Kiểm tra cả thời gian đến dự kiến.");
        var drivers=new[]{f.DriverId,f.CoDriverId}.OfType<string>().ToArray();
        if(drivers.Length>0)Guard.Rule(!await others.AnyAsync(x=>
            x.DriverId!=null&&drivers.Contains(x.DriverId)||x.CoDriverId!=null&&drivers.Contains(x.CoDriverId),ct),"Tài xế bị trùng lịch.");
        if(f.StaffId!=null)Guard.Rule(!await others.AnyAsync(x=>x.StaffId==f.StaffId,ct),"Nhân viên bị trùng lịch.");
        if(f.Id!=0&&trip.VehicleId!=f.VehicleId) {var old=await db.TripSeats.Where(x=>x.TripId==f.Id).ToListAsync(ct);db.TripSeats.RemoveRange(old);await db.SaveChangesAsync(ct);}
        bool newSeats=f.Id==0||trip.VehicleId!=f.VehicleId;
        trip.BusRouteId=f.BusRouteId;trip.VehicleId=f.VehicleId;trip.DepartureUtc=start;trip.ArrivalUtc=end;trip.Fare=f.Fare;trip.DriverId=f.DriverId;trip.CoDriverId=f.CoDriverId;trip.StaffId=f.StaffId;
        if(newSeats) foreach(var s in vehicle.Layout.Seats)trip.Seats.Add(new TripSeat{PhysicalSeatId=s.Id,Code=s.Code,Row=s.Row,Column=s.Column,Floor=s.Floor,Status=SeatStatus.Available});
        if(f.Id==0)db.Trips.Add(trip);trip.Revision++;
        gate.Audit(actor,"TripSaved",f.Id.ToString(),now);await db.SaveChangesAsync(ct);
        if(f.Id==0&&copyStopsFrom is int sourceId){
            var source=await gate.Trip(sourceId,ct);
            Guard.Rule(source.BusRouteId==trip.BusRouteId&&TripStopService.Duration(source)==TripStopService.Duration(trip),"Lịch điểm không khớp chuyến mới.");
            var rows=await db.TripStops.AsNoTracking().Where(s=>s.TripId==sourceId&&s.IsActive).ToListAsync(ct);
            db.TripStops.AddRange(rows.Select(s=>new TripStop{
                TripId=trip.Id,StopOrder=s.StopOrder,Name=s.Name,Address=s.Address,OffsetMinutes=s.OffsetMinutes,
                CanPickup=s.CanPickup,CanDropoff=s.CanDropoff,IsActive=true,IsIllustrative=s.IsIllustrative
            }));
            await db.SaveChangesAsync(ct);
        }else if(f.Id==0||structural)await TripStopService.EnsureEndpoints(db,trip,route,ct);
        if(f.Id==0&&copyStopsFrom is null) {
            var stops=await db.TripStops.AsNoTracking().Where(x=>x.TripId==trip.Id).OrderBy(x=>x.StopOrder).ToListAsync(ct);
            var plan=StopAppendPlanner.Dalat(trip,route,stops,now);
            if(plan.Ready) {
                db.TripStops.AddRange(plan.NewStops);
                gate.Audit(actor,"TripStopDefaultsAdded",trip.Id.ToString(),now,"source=dalat-420-v1; illustrative=true");
                await db.SaveChangesAsync(ct);
            }
        }
        return trip;
    }
    public async Task SetRole(string actor,RoleForm f,CancellationToken ct=default) {
        Guard.Input(Roles.All.Contains(f.Role),"Vai trò không hợp lệ.");Guard.Rule(actor!=f.UserId,"Không tự thay vai trò/khóa chính tài khoản đang đăng nhập.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);await gate.Catalog(ct);
        var user=Guard.Found(await users.FindByIdAsync(f.UserId));var current=await users.GetRolesAsync(user);var now=await gate.Now(ct);
        if(!f.IsActive||!current.Contains(f.Role)) await EnsureUnassigned(user.Id,ct);
        Check(await users.RemoveFromRolesAsync(user,current));Check(await users.AddToRoleAsync(user,f.Role));
        user.IsActive=f.IsActive;Check(await users.UpdateAsync(user));Check(await users.UpdateSecurityStampAsync(user));
        gate.Audit(actor,"UserRoleChanged",user.Id,now,f.Role+";Active="+f.IsActive);await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    private async Task EnsureUnassigned(string userId,CancellationToken ct) {
        var assigned=await db.Trips.AnyAsync(x=>(x.DriverId==userId||x.CoDriverId==userId||x.StaffId==userId)
            &&x.Status!=TripStatus.Completed&&x.Status!=TripStatus.Cancelled,ct);
        Guard.Rule(!assigned,"Người dùng còn được phân công chuyến. Hãy cập nhật phân công trước khi ngừng hoạt động hoặc đổi vai trò.");
    }
    private static void Check(IdentityResult r)=>Guard.Input(r.Succeeded,string.Join("; ",r.Errors.Select(x=>x.Description)));
}
