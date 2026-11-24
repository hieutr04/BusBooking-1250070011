using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using BusBooking.Data;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Services;

public sealed record SpecificTripPlan(int TripId,long Revision,List<SpecificStopRequest> Points);
public sealed record SpecificPlanFile(List<SpecificTripPlan> Trips);
public static class SpecificStopMaintenance {
 public static async Task<bool> Run(AppDbContext db,TripStopAppendService service,string input,bool dryRun,string report,CancellationToken ct=default) {
  if((await db.Database.GetPendingMigrationsAsync(ct)).Any())throw new InvalidOperationException("Áp dụng đúng migration còn thiếu trước. Không seed hoặc ghi giả history.");
  Guard.Input(Path.GetFullPath(input)!=Path.GetFullPath(report),"Tệp báo cáo phải khác tệp kế hoạch.");
  Guard.Input(Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(report))),"Thư mục báo cáo phải tồn tại và được phép ghi.");
  var file=new FileInfo(input);Guard.Input(file.Exists&&file.Length<=2_000_000,"Tệp kế hoạch phải tồn tại và không quá 2 MB.");
  var options=new JsonSerializerOptions{PropertyNameCaseInsensitive=true,UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow,WriteIndented=true};
  var plan=JsonSerializer.Deserialize<SpecificPlanFile>(await File.ReadAllTextAsync(input,ct),options);
  Guard.Input(plan?.Trips!=null&&plan.Trips.Count is >0 and <=1000,"Tệp cần từ 1 đến 1000 chuyến.");
  Guard.Input(plan!.Trips.All(x=>x!=null&&x.TripId>0&&x.Revision>=0&&x.Points!=null&&x.Points.All(p=>p!=null))&&plan.Trips.Select(x=>x.TripId).Distinct().Count()==plan.Trips.Count,"Không lặp chuyến hoặc để thiếu thông tin duyệt.");
  async Task<object?> Read(int id) {
   var trip=await db.Trips.AsNoTracking().Where(x=>x.Id==id).Select(x=>new{x.Id,x.BusRouteId,x.VehicleId,x.DepartureUtc,x.ArrivalUtc,x.Fare,x.Status,x.SalesOpen,x.DriverId,x.StaffId}).SingleOrDefaultAsync(ct);
   if(trip==null)return null;
   var geometryHash=Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(trip)));
   var stops=await db.TripStops.AsNoTracking().Where(x=>x.TripId==id).OrderBy(x=>x.StopOrder).Select(x=>new{x.Id,x.StopOrder,x.Name,x.Address,x.OffsetMinutes,x.CanPickup,x.CanDropoff,x.IsActive,x.IsIllustrative}).ToListAsync(ct);
   return new{geometryHash,stops};
  }
  var records=new List<object>();var ok=true;
  foreach(var entry in plan.Trips) {
   db.ChangeTracker.Clear();var before=await Read(entry.TripId);
   try {
    var result=await service.Specific("command",entry.TripId,entry.Revision,entry.Points,dryRun,ct);
    if(result.Plan.Blocked)ok=false;
    records.Add(new{entry.TripId,status=result.Applied?"applied":result.Plan.Ready?"planned":result.Plan.Status,result.BeforeRevision,result.AfterRevision,result.Plan.Message,before,after=await Read(entry.TripId),proposed=result.Plan.NewStops});
   }catch(BusinessException e){ok=false;records.Add(new{entry.TripId,status="blocked",reason=e.Message,before,after=await Read(entry.TripId)});}
   catch {
    // Earlier per-trip commits are real; preserve their report before propagating an unexpected SQL/IO failure.
    records.Add(new{entry.TripId,status="failed",reason="Không thể hoàn tất chuyến này. Kiểm tra log; các chuyến đã commit trước đó được giữ nguyên.",before});
    await File.WriteAllTextAsync(report,JsonSerializer.Serialize(new{dryRun,utc=DateTime.UtcNow,records},options),ct);throw;
   }
  }
  await File.WriteAllTextAsync(report,JsonSerializer.Serialize(new{dryRun,utc=DateTime.UtcNow,note="Before/after are observed reads outside each short write transaction. Concurrent legitimate activity can change geometry; stop/hold preservation is separately covered by SQL regression tests.",records},options),ct);
  Console.WriteLine($"{(dryRun?"Đã xem trước":"Đã xử lý")} {records.Count} chuyến. {(ok?"Không có mục bị chặn.":"Có mục bị chặn; xem từng lý do, không phải tất cả đã cập nhật.")} Báo cáo: {Path.GetFullPath(report)}");return ok;
 }
}
