using BusBooking.Models;
using BusBooking.Services;
using BusBooking.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Data;

public static class PairedFleetSchedule {
    private const string Actor="paired-fleet";

    public static async Task Run(IServiceProvider services) {
        Guard.Rule(services.GetRequiredService<IWebHostEnvironment>().IsDevelopment(),"Chỉ tạo đội xe đồ án trong Development.");
        var db=services.GetRequiredService<AppDbContext>();
        var catalog=services.GetRequiredService<CatalogService>();
        var users=services.GetRequiredService<UserManager<AppUser>>();
        Guard.Rule(!(await db.Database.GetPendingMigrationsAsync()).Any(),"Cần cập nhật migration tài xế thứ hai.");
        var now=await services.GetRequiredService<DbGate>().Now();
        var dayStart=BusinessTime.FromVietnam(BusinessTime.Vietnam(now).Date);
        var trips=await db.Trips.AsNoTracking().Include(t=>t.Vehicle).ThenInclude(v=>v.Layout)
            .Where(t=>t.Route.IsActive&&t.SalesOpen&&t.Status==TripStatus.Scheduled&&t.DepartureUtc>=dayStart)
            .OrderBy(t=>t.DepartureUtc).ToListAsync();
        var groups=trips.GroupBy(t=>t.BusRouteId).OrderBy(g=>g.Key).ToArray();
        Guard.Rule(groups.Length==9,"Cần đúng 9 tuyến đang có lịch tương lai để tạo đội xe này.");
        var days=groups.SelectMany(g=>g.GroupBy(t=>BusinessTime.Vietnam(t.DepartureUtc).Date)).ToArray();
        Guard.Rule(days.All(g=>g.Count()<=2&&(g.Count()!=2||g.Last().DepartureUtc-g.First().DepartureUtc==TimeSpan.FromHours(1))),
            "Có ngày đã có nhiều lượt hoặc hai lượt chưa cách đúng 1 giờ. Kiểm tra lịch trước khi bổ sung.");
        var twoDeck24=Guard.Found(await db.Layouts.AsNoTracking().SingleOrDefaultAsync(x=>x.Name=="Giường nằm 24 chỗ / 2 tầng"&&x.IsActive));
        const string layoutName="Giường nằm 32 chỗ / 2 tầng";
        var layout32=await db.Layouts.AsNoTracking().SingleOrDefaultAsync(x=>x.Name==layoutName);
        if(layout32 is null){
            await catalog.SaveLayout(Actor,new LayoutForm{Name=layoutName,Rows=8,Columns=2,Floors=2,IsActive=true});
            layout32=await db.Layouts.AsNoTracking().SingleAsync(x=>x.Name==layoutName);
        }
        Guard.Rule(layout32.IsActive&&layout32.Rows==8&&layout32.Columns==2&&layout32.Floors==2,"Sơ đồ 32 giường đang có khác cấu hình yêu cầu.");

        var password=Environment.GetEnvironmentVariable("BUSCENTER_FLEET_DEMO_PASSWORD");
        Guard.Input(!string.IsNullOrWhiteSpace(password),"Đặt BUSCENTER_FLEET_DEMO_PASSWORD cho các tài khoản đồ án mới.");
        var driverIds=new List<string>();
        for(var i=1;i<=41;i++){
            var email=$"fleet-driver-{i:00}@busbooking.test";
            var user=await users.FindByEmailAsync(email);
            if(user is null){
                user=new AppUser{UserName=email,Email=email,EmailConfirmed=true,
                    FullName=i switch {
                        1=>"Nguyễn Văn A",
                        2=>"Trần Bảo A",
                        <=36=>$"Tài xế đội xe {i:00}",
                        _=>$"Tài xế dự phòng {i-36:00}"
                    }};
                Check(await users.CreateAsync(user,password!));
            }
            if(!await users.IsInRoleAsync(user,Roles.Driver)){
                Guard.Rule(!(await users.GetRolesAsync(user)).Any(),"Tài khoản đội xe đã có vai trò khác.");
                Check(await users.AddToRoleAsync(user,Roles.Driver));
            }
            Guard.Rule(user.IsActive,"Tài khoản đội xe đang bị khóa.");
            driverIds.Add(user.Id);
        }

        var protectedIds=await db.SeatHolds.Select(h=>h.TripId).Distinct().ToListAsync();
        var history=protectedIds.ToHashSet();
        var previousVehicles=trips.Select(t=>t.VehicleId).Distinct().ToArray();
        var created=0;
        var preserved=0;
        for(var routeIndex=0;routeIndex<groups.Length;routeIndex++){
            var group=groups[routeIndex];
            var use24=group.Any(t=>t.Vehicle.Layout.Rows*t.Vehicle.Layout.Columns*t.Vehicle.Layout.Floors==24&&t.Vehicle.Layout.Floors==2);
            var layout=use24?twoDeck24:layout32;
            var vehicles=new int[2];
            for(var slot=0;slot<2;slot++){
                var plate=$"FLEET-{group.Key:00}-{slot+1}";
                var vehicle=await db.Vehicles.AsNoTracking().SingleOrDefaultAsync(v=>v.Plate==plate);
                if(vehicle is null){
                    await catalog.SaveVehicle(Actor,new VehicleForm{Plate=plate,Name=use24?"Xe giường nằm 24 chỗ":"Xe giường nằm 32 chỗ",SeatLayoutId=layout.Id,IsActive=true});
                    vehicle=await db.Vehicles.AsNoTracking().SingleAsync(v=>v.Plate==plate);
                }
                Guard.Rule(vehicle.IsActive&&vehicle.SeatLayoutId==layout.Id,"Xe đội mới đang ngừng hoạt động hoặc dùng sơ đồ khác.");
                vehicles[slot]=vehicle.Id;
            }
            foreach(var day in group.GroupBy(t=>BusinessTime.Vietnam(t.DepartureUtc).Date)){
                var scheduled=day.ToArray();
                var first=scheduled[0];
                if(first.DepartureUtc<=now)continue;
                for(var slot=0;slot<2;slot++){
                    var existing=scheduled.ElementAtOrDefault(slot);
                    var keepVehicle=existing is not null&&history.Contains(existing.Id);
                    if(keepVehicle&&existing!.VehicleId!=vehicles[slot])preserved++;
                    var original=existing??first;
                    var offset=existing is null?60:0;
                    var crew=routeIndex*4+slot*2;
                    await catalog.SaveTrip(Actor,new TripForm{
                        Id=existing?.Id??0,BusRouteId=group.Key,VehicleId=keepVehicle?original.VehicleId:vehicles[slot],
                        DepartureLocal=BusinessTime.Vietnam(original.DepartureUtc).AddMinutes(offset),
                        ArrivalLocal=BusinessTime.Vietnam(original.ArrivalUtc).AddMinutes(offset),Fare=original.Fare,
                        DriverId=driverIds[crew],CoDriverId=driverIds[crew+1],StaffId=existing?.StaffId
                    },copyStopsFrom:existing is null?first.Id:null);
                    if(existing is null)created++;
                }
            }
        }

        foreach(var vehicle in await db.Vehicles.AsNoTracking().Where(v=>previousVehicles.Contains(v.Id)&&v.Plate.StartsWith("TEST-")&&v.IsActive).ToListAsync()){
            if(await db.Trips.AnyAsync(t=>t.VehicleId==vehicle.Id&&t.Status!=TripStatus.Cancelled&&t.ArrivalUtc>now))continue;
            await catalog.SaveVehicle(Actor,new VehicleForm{Id=vehicle.Id,Plate=vehicle.Plate,Name=vehicle.Name,SeatLayoutId=vehicle.SeatLayoutId,IsActive=false});
        }
        Console.WriteLine($"Đã bổ sung {created} lượt; đội 41 tài xế, trong đó 5 dự phòng. Có {preserved} lượt giữ xe cũ vì lịch sử giữ chỗ.");
    }

    private static void Check(IdentityResult result)=>Guard.Rule(result.Succeeded,string.Join("; ",result.Errors.Select(e=>e.Description)));
}
