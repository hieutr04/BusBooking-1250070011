using BusBooking.Models;
using BusBooking.Services;
using BusBooking.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Data;
public static class SeedData {
    public static async Task Run(IServiceProvider services) {
        var env=services.GetRequiredService<IWebHostEnvironment>();if(!env.IsDevelopment())throw new InvalidOperationException("Seed chỉ được chạy trong Development.");
        var db=services.GetRequiredService<AppDbContext>();var users=services.GetRequiredService<UserManager<AppUser>>();var roles=services.GetRequiredService<RoleManager<IdentityRole>>();
        if((await db.Database.GetPendingMigrationsAsync()).Any())throw new InvalidOperationException("Chạy dotnet ef database update trước khi seed.");
        const string password="BusStudy!2026";
        foreach(var role in Models.Roles.All) {
            if(!await roles.RoleExistsAsync(role))Check(await roles.CreateAsync(new IdentityRole(role)));
            var mail=role.ToLowerInvariant()+"@busbooking.test";var user=await users.FindByEmailAsync(mail);
            if(user==null) {
                user=new AppUser{UserName=mail,Email=mail,EmailConfirmed=true,FullName=role+" — tài khoản thử nghiệm"};
                Check(await users.CreateAsync(user,password));Check(await users.AddToRoleAsync(user,role));
            }
        }
        foreach(var tuple in new[]{("TP. Hồ Chí Minh","Đà Lạt",420),("TP. Hồ Chí Minh","Nha Trang",480),("Đà Lạt","TP. Hồ Chí Minh",420)}) {
            if(!await db.Routes.AnyAsync(x=>x.Origin==tuple.Item1&&x.Destination==tuple.Item2)) {
                db.Routes.Add(new BusRoute{Origin=tuple.Item1,Destination=tuple.Item2,DurationMinutes=tuple.Item3,Description="Dữ liệu học tập; không phải lịch/giá bán của doanh nghiệp."});await db.SaveChangesAsync();
            }
        }
        var catalog=services.GetRequiredService<CatalogService>();
        if(!await db.Layouts.AnyAsync(x=>x.Name=="Ghế ngồi 32 chỗ"))await catalog.SaveLayout("seed",new LayoutForm{Name="Ghế ngồi 32 chỗ",Rows=8,Columns=4, Floors=1});
        if(!await db.Layouts.AnyAsync(x=>x.Name=="Giường nằm 24 chỗ / 2 tầng"))await catalog.SaveLayout("seed",new LayoutForm{Name="Giường nằm 24 chỗ / 2 tầng",Rows=4,Columns=3,Floors=2});
        var layouts=await db.Layouts.OrderBy(x=>x.Id).ToListAsync();
        for(int i=1;i<=3;i++)if(!await db.Vehicles.AnyAsync(x=>x.Plate==$"TEST-{i:000}"))await catalog.SaveVehicle("seed",new VehicleForm{Plate=$"TEST-{i:000}",Name=i==2?"Xe giường nằm — dữ liệu thử":"Xe ghế ngồi — dữ liệu thử",SeatLayoutId=layouts[i==2?1:0].Id});
        var routes=await db.Routes.OrderBy(x=>x.Id).Take(3).ToListAsync();var vehicles=await db.Vehicles.Where(x=>x.Plate.StartsWith("TEST-")).OrderBy(x=>x.Plate).ToListAsync();
        var driver=await users.FindByEmailAsync("driver@busbooking.test");var staff=await users.FindByEmailAsync("staff@busbooking.test");
        var today=DateTime.UtcNow.AddHours(7).Date;
        for(int day=1;day<=14;day++)for(int i=0;i<routes.Count;i++) {
            var local=today.AddDays(day).AddHours(7+i*2);var utc=BusinessTime.FromVietnam(local);
            if(await db.Trips.AnyAsync(x=>x.BusRouteId==routes[i].Id&&x.VehicleId==vehicles[i].Id&&x.DepartureUtc==utc))continue;
            await catalog.SaveTrip("seed",new TripForm{BusRouteId=routes[i].Id,VehicleId=vehicles[i].Id,DepartureLocal=local,ArrivalLocal=local.AddMinutes(routes[i].DurationMinutes),Fare=180000+i*70000,DriverId=i==0?driver!.Id:null,StaffId=i==0?staff!.Id:null});
        }
        // Create one real SQL-backed booking via exactly the same service used by web and staff.
        var customer=(await users.FindByEmailAsync("customer@busbooking.test"))!;
        if(!await db.Bookings.AnyAsync(x=>x.OwnerId==customer.Id)) {
            var trip=await db.Trips.OrderBy(x=>x.DepartureUtc).FirstAsync(x=>x.DepartureUtc>DateTime.UtcNow&&x.SalesOpen&&x.Status==TripStatus.Scheduled);
            var seat=await db.TripSeats.FirstAsync(x=>x.TripId==trip.Id&&x.Status==SeatStatus.Available);
            var service=services.GetRequiredService<BookingService>();var hold=await service.Acquire(customer.Id,new HoldForm{TripId=trip.Id,SeatIds=[seat.Id],RequestKey=Guid.NewGuid()});
            var booking=await service.Confirm(customer.Id,false,new ConfirmForm{HoldId=hold.Id,ContactName="Khách thử nghiệm",Phone="0900000000",Passengers=[new PassengerForm{TripSeatId=seat.Id,Name="Hành khách thử nghiệm"}]});
            await service.Money(staff!.Id,new PaymentForm{BookingId=booking,RequestKey=Guid.NewGuid(),Note="Khoản thu mô phỏng để kiểm báo cáo."});
        }
        Console.WriteLine("Seed development hoàn tất. Không reset dữ liệu hay mật khẩu đã có.");
    }
    private static void Check(IdentityResult r) {if(!r.Succeeded)throw new InvalidOperationException(string.Join(";",r.Errors.Select(x=>x.Description)));}
}
