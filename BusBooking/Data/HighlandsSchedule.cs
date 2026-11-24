using BusBooking.Services;
using BusBooking.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Data;

public static class HighlandsSchedule {
    private const string City="TP. Hồ Chí Minh";
    private const string Actor="highlands-schedule";

    public static async Task Run(IServiceProvider services) {
        if(!services.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            throw new InvalidOperationException("Lịch xe đồ án chỉ được tạo trong Development.");
        var db=services.GetRequiredService<AppDbContext>();
        var catalog=services.GetRequiredService<CatalogService>();
        if((await db.Database.GetPendingMigrationsAsync()).Any())
            throw new InvalidOperationException("Áp dụng migration đang chờ trước khi thêm chuyến.");

        var layoutId=await db.Layouts.Where(x=>x.IsActive).OrderBy(x=>x.Id)
            .Select(x=>x.Id).FirstOrDefaultAsync();
        Guard.Rule(layoutId>0,"Cần có sơ đồ ghế đang hoạt động.");
        var today=BusinessTime.Vietnam(await services.GetRequiredService<DbGate>().Now()).Date;
        var created=0;
        var destinations=new[]{
            (Place:"Đắk Lắk",Plate:"TEST-TN-01",Minutes:480,Fare:300000m),
            (Place:"Gia Lai",Plate:"TEST-TN-02",Minutes:540,Fare:350000m),
            (Place:"Quảng Ngãi",Plate:"TEST-TN-03",Minutes:600,Fare:380000m)
        };
        foreach(var item in destinations){
            var vehicle=await db.Vehicles.AsNoTracking().SingleOrDefaultAsync(x=>x.Plate==item.Plate);
            if(vehicle is null){
                await catalog.SaveVehicle(Actor,new VehicleForm{Plate=item.Plate,Name="Xe tuyến Tây Nguyên — dữ liệu thử",SeatLayoutId=layoutId,IsActive=true});
                vehicle=await db.Vehicles.AsNoTracking().SingleAsync(x=>x.Plate==item.Plate);
            }
            Guard.Rule(vehicle.IsActive,"Xe dùng cho lịch Tây Nguyên đã ngừng hoạt động.");
            foreach(var direction in new[]{(Origin:City,Destination:item.Place,Hour:5),(Origin:item.Place,Destination:City,Hour:16)}){
                var route=await db.Routes.AsNoTracking().SingleOrDefaultAsync(x=>x.Origin==direction.Origin&&x.Destination==direction.Destination);
                if(route is null){
                    await catalog.SaveRoute(Actor,new RouteForm{
                        Origin=direction.Origin,Destination=direction.Destination,DurationMinutes=item.Minutes,
                        IsActive=true,Description="Dữ liệu học tập; không phải lịch/giá bán của doanh nghiệp."
                    });
                    route=await db.Routes.AsNoTracking().SingleAsync(x=>x.Origin==direction.Origin&&x.Destination==direction.Destination);
                }
                Guard.Rule(route.IsActive,"Tuyến Tây Nguyên hiện đang ngừng hoạt động.");
                for(var day=1;day<=7;day++){
                    var departure=today.AddDays(day).AddHours(direction.Hour).AddMinutes(30);
                    var utc=BusinessTime.FromVietnam(departure);
                    if(await db.Trips.AnyAsync(x=>x.BusRouteId==route.Id&&x.VehicleId==vehicle.Id&&x.DepartureUtc==utc))continue;
                    await catalog.SaveTrip(Actor,new TripForm{
                        BusRouteId=route.Id,VehicleId=vehicle.Id,DepartureLocal=departure,
                        ArrivalLocal=departure.AddMinutes(route.DurationMinutes),Fare=item.Fare
                    });
                    created++;
                }
            }
        }
        Console.WriteLine($"Đã tạo {created} chuyến đồ án Tây Nguyên.");
    }
}
