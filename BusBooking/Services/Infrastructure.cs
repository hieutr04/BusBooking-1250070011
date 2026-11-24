using System.Data;
using System.Data.Common;
using BusBooking.Data;
using BusBooking.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
namespace BusBooking.Services;
public class BusinessException(int status,string message):Exception(message) { public int Status {get;}=status; }
public static class Guard {
    public static void Rule(bool ok,string message) {if(!ok)throw new BusinessException(409,message);}
    public static void Input(bool ok,string message) {if(!ok)throw new BusinessException(400,message);}
    public static T Found<T>(T? value) where T:class =>value??throw new BusinessException(404,"Không tìm thấy dữ liệu.");
}
public class BookingOptions {
    public int HoldSeconds {get;set;}=300;
    public int ArriveEarlyMinutes {get;set;}=15;
    public int ExpiryScanSeconds {get;set;}=5;
    public int ExpiryBatchSize {get;set;}=50;
    public int MaxSeats {get;set;}=6;
    public int ScheduleBufferMinutes {get;set;}
}
public static class BusinessTime {
    public static DateTime Utc(DateTime value)=>DateTime.SpecifyKind(value,DateTimeKind.Utc);
    public static DateTime Vietnam(DateTime value)=>Utc(value).AddHours(7);
    public static DateTime FromVietnam(DateTime local)=>DateTime.SpecifyKind(local.AddHours(-7),DateTimeKind.Utc);
    public static string Text(DateTime value)=>Vietnam(value).ToString("dd/MM/yyyy HH:mm");
    public static string Money(decimal value)=>value.ToString("N0",System.Globalization.CultureInfo.GetCultureInfo("vi-VN"))+" ₫";
    public static string TripLabel(TripStatus s)=>s switch {TripStatus.Scheduled=>"Sắp khởi hành",TripStatus.Boarding=>"Chuẩn bị chạy",TripStatus.InProgress=>"Đang chạy",TripStatus.Completed=>"Hoàn thành",_=>"Đã hủy"};
}
public class DbGate(AppDbContext db) {
    public async Task<DateTime> Now(CancellationToken ct=default)=>BusinessTime.Utc(await db.Database.SqlQueryRaw<DateTime>("SELECT SYSUTCDATETIME() AS [Value]").SingleAsync(ct));
    public async Task<Trip> Trip(int id,CancellationToken ct=default)=>Guard.Found(await db.Trips.FromSqlInterpolated($"SELECT * FROM dbo.Trips WITH (UPDLOCK, ROWLOCK) WHERE Id={id}").SingleOrDefaultAsync(ct));
    public async Task<List<TripSeat>> Seats(int tripId,IEnumerable<int> ids,CancellationToken ct=default) {
        var result=new List<TripSeat>();
        foreach(var id in ids.Distinct().Order()) {
            var seat=await db.TripSeats.FromSqlInterpolated($"SELECT * FROM dbo.TripSeats WITH (UPDLOCK, ROWLOCK) WHERE Id={id} AND TripId={tripId}").SingleOrDefaultAsync(ct);
            Guard.Input(seat!=null,"Ghế không thuộc chuyến này.");result.Add(seat!);
        }
        return result;
    }
    public async Task Catalog(CancellationToken ct=default) {
        // One database-scoped scheduling/configuration gate shared by all app instances.
        var connection=db.Database.GetDbConnection();
        using var cmd=connection.CreateCommand();cmd.Transaction=db.Database.CurrentTransaction!.GetDbTransaction();
        cmd.CommandText="DECLARE @r int; EXEC @r=sp_getapplock @Resource=N'BusBooking.Catalog', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000; SELECT @r";
        var value=Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        Guard.Rule(value>=0,"Danh mục đang được cập nhật. Hãy thử lại.");
    }
    public void Audit(string actor,string action,string entity,DateTime now,string detail="")=>db.AuditEntries.Add(new AuditEntry{ActorId=actor,Action=action,EntityId=entity,CreatedUtc=now,Detail=detail});
    public static void Sellable(Trip t,DateTime now)=>Guard.Rule(t.Status==TripStatus.Scheduled&&t.SalesOpen&&t.DepartureUtc>now,"Chuyến đã đóng bán, khởi hành hoặc không còn hợp lệ.");
}
