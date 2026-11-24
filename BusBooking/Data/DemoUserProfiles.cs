using BusBooking.Models;
using BusBooking.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Data;

public static class DemoUserProfiles {
    private static readonly string[] FleetNames=[
        "Nguyễn Văn A","Trần Bảo A","Lê Hữu An","Phạm Minh Bảo","Võ Quốc Bình",
        "Đặng Văn Cường","Bùi Thành Dũng","Đỗ Minh Đức","Hồ Quốc Hải","Phan Văn Hiệp",
        "Vũ Hoàng Hưng","Ngô Thành Khải","Dương Văn Khoa","Hoàng Đức Lâm","Lý Thành Lợi",
        "Mai Quang Minh","Đinh Hữu Nam","Tạ Hoàng Nghĩa","Cao Văn Nhân","Trương Quốc Phát",
        "Nguyễn Hữu Phong","Trần Quang Phú","Lê Văn Quân","Phạm Thành Sang","Võ Minh Sơn",
        "Đặng Quốc Tài","Bùi Văn Tâm","Đỗ Hữu Thắng","Hồ Minh Thiện","Phan Quốc Thịnh",
        "Vũ Văn Thông","Ngô Hữu Thuận","Dương Thành Tiến","Hoàng Văn Toàn","Lý Quốc Trí",
        "Mai Hữu Trung","Đinh Quang Tuấn","Tạ Văn Tùng","Cao Thành Việt","Trương Hữu Vinh",
        "Nguyễn Quốc Vũ"
    ];

    public static async Task Run(IServiceProvider services) {
        Guard.Rule(services.GetRequiredService<IWebHostEnvironment>().IsDevelopment(),
            "Chỉ khởi tạo hồ sơ và mật khẩu mẫu trong Development.");
        var password=Environment.GetEnvironmentVariable("BUSCENTER_PROFILE_PASSWORD");
        Guard.Input(!string.IsNullOrWhiteSpace(password),"Đặt BUSCENTER_PROFILE_PASSWORD trước khi cấp mật khẩu mẫu.");
        var db=services.GetRequiredService<AppDbContext>();
        var users=services.GetRequiredService<UserManager<AppUser>>();
        var gate=services.GetRequiredService<DbGate>();
        Guard.Rule(!(await db.Database.GetPendingMigrationsAsync()).Any(),"Cập nhật migration hồ sơ trước.");

        await using var tx=await db.Database.BeginTransactionAsync();
        await gate.Catalog();
        var now=await gate.Now();
        var count=0;
        foreach(var profile in Profiles()) {
            var user=await users.FindByEmailAsync(profile.Email);
            if(user is null)continue;
            var duplicate=await users.FindByNameAsync(profile.UserName);
            Guard.Rule(duplicate is null||duplicate.Id==user.Id,"Tên đăng nhập "+profile.UserName+" đã thuộc tài khoản khác.");
            if(IsPlaceholder(user.FullName))user.FullName=profile.FullName;
            if(string.IsNullOrWhiteSpace(user.PhoneNumber))user.PhoneNumber=profile.Phone;
            user.DateOfBirth??=profile.Birth;
            if(string.IsNullOrWhiteSpace(user.Address))user.Address=profile.Address;
            if(string.IsNullOrWhiteSpace(user.ProvinceCode))user.ProvinceCode=profile.ProvinceCode;
            user.UserName=profile.UserName;
            user.AccessFailedCount=0;
            user.LockoutEnd=null;
            Check(await users.UpdateAsync(user));
            var token=await users.GeneratePasswordResetTokenAsync(user);
            Check(await users.ResetPasswordAsync(user,token,password!));
            gate.Audit("profile-seed","UserProfileInitialized",user.Id,now,"Username="+profile.UserName);
            count++;
        }
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        Console.WriteLine($"Đã bổ sung hồ sơ và cấp thông tin đăng nhập cho {count} tài khoản mẫu.");
    }

    private static bool IsPlaceholder(string name)=>
        string.IsNullOrWhiteSpace(name)||name.Contains("tài khoản thử nghiệm",StringComparison.OrdinalIgnoreCase)
        ||name.StartsWith("Tài xế đội xe ",StringComparison.Ordinal)
        ||name.StartsWith("Tài xế dự phòng ",StringComparison.Ordinal);

    private static IEnumerable<DemoProfile> Profiles() {
        yield return new("admin@busbooking.test","admin","Admin HCM","0000009001",
            new(1988,4,15),"365 Huỳnh Tấn Phát, phường Tân Thuận","79");
        yield return new("staff@busbooking.test","staff","Nhân viên HCM","0000009002",
            new(1995,8,20),"365 Huỳnh Tấn Phát, phường Tân Thuận","79");
        yield return new("customer@busbooking.test","customer","Nguyễn Văn A","0903000001",
            new(2000,5,12),"25 Nguyễn Văn Cừ","79");
        yield return new("driver@busbooking.test","driver","Trần Bảo A","0000009003",
            new(1985,3,18),"42 Lê Văn Việt","79");
        for(var i=1;i<=41;i++) {
            yield return new($"fleet-driver-{i:00}@busbooking.test",$"taixe{i:00}",FleetNames[i-1],
                $"000002{i:0000}",new DateOnly(1975+i%21,1+i%12,1+i%27),
                $"{20+i} Nguyễn Văn Cừ","79");
        }
        for(var i=1;i<=40;i++) {
            yield return new($"driver-demo-{i:00}@busbooking.test",$"taixe{i+41:00}","Tài xế",
                $"000001{i:0000}",new DateOnly(1978+i%20,1+i%12,1+i%27),
                $"{50+i} Nguyễn Văn Cừ","79");
        }
    }

    private static void Check(IdentityResult result)=>
        Guard.Input(result.Succeeded,string.Join("; ",result.Errors.Select(error=>error.Description)));

    private sealed record DemoProfile(string Email,string UserName,string FullName,string Phone,
        DateOnly Birth,string Address,string ProvinceCode);
}
