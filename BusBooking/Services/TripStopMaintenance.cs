using System.Globalization;
using BusBooking.Data;
using BusBooking.Models;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Services;

public static class TripStopMaintenance {
    // Explicit command, not startup seeding. One short transaction per trip, shared with Acquire/Confirm.
    public static async Task Run(AppDbContext db,DbGate gate,bool study,string reportPath,CancellationToken ct=default) {
        if((await db.Database.GetPendingMigrationsAsync(ct)).Any())throw new InvalidOperationException("Áp dụng migrations còn thiếu trước khi cập nhật lịch điểm.");
        var ids=await db.Trips.AsNoTracking().OrderBy(t=>t.Id).Select(t=>t.Id).ToListAsync(ct);
        var report=new List<string>{"TripId,Route,DepartureUtc,ArrivalUtc,Fare,StopOrder,StopName,AtUtc,Illustrative,Action"};
        foreach(var id in ids) {
            await using var tx=await db.Database.BeginTransactionAsync(ct);
            var trip=await gate.Trip(id,ct);var route=await db.Routes.SingleAsync(r=>r.Id==trip.BusRouteId,ct);
            var vehicle=await db.Vehicles.AsNoTracking().SingleAsync(v=>v.Id==trip.VehicleId,ct);
            var action="kept";
            var stops=await db.TripStops.Where(s=>s.TripId==id).OrderBy(s=>s.StopOrder).ToListAsync(ct);
            var future=trip.Status==TripStatus.Scheduled&&trip.DepartureUtc>await gate.Now(ct);
            // The dedicated insert-only path below the legacy command handles this direction.
            // Do not let the old pristine/no-hold branch modify a Dalat schedule first.
            if(study&&StopAppendPlanner.DalatRoute(route)) {
                await tx.CommitAsync(ct);db.ChangeTracker.Clear();continue;
            }
            if(stops.Count==0&&future) {await TripStopService.EnsureEndpoints(db,trip,route,ct);action="endpoints-added";
                stops=await db.TripStops.Where(s=>s.TripId==id).OrderBy(s=>s.StopOrder).ToListAsync(ct);}
            var locked=!future||await db.SeatHolds.AnyAsync(h=>h.TripId==id,ct);
            var pristine=stops.Count==2&&stops[0].Name=="Điểm đầu · "+route.Origin&&stops[1].Name=="Điểm cuối · "+route.Destination&&
                stops[0].StopOrder==0&&stops[1].StopOrder==1000&&stops.All(s=>s.Address==null&&s.IsActive&&s.IsIllustrative)&&
                stops[0].OffsetMinutes==0&&stops[1].OffsetMinutes==TripStopService.Duration(trip);
            if(study&&!locked&&pristine&&vehicle.Plate.StartsWith("TEST-",StringComparison.Ordinal)&&
                route.Description=="Dữ liệu học tập; không phải lịch/giá bán của doanh nghiệp.") {
                var template=StudyTemplate(route.Origin,route.Destination,TripStopService.Duration(trip));
                foreach(var (name,minutes) in template)db.TripStops.Add(new TripStop{TripId=id,StopOrder=minutes,
                    Name="Điểm học tập · "+name,OffsetMinutes=minutes,CanPickup=true,CanDropoff=true,IsIllustrative=true});
                if(template.Length>0)action="study-stops-added";
            }
            if(action!="kept"){trip.Revision++;gate.Audit("command","TripStopsUpdated",id.ToString(),await gate.Now(ct),action);await db.SaveChangesAsync(ct);}
            await tx.CommitAsync(ct);
            foreach(var stop in await db.TripStops.AsNoTracking().Where(s=>s.TripId==id).OrderBy(s=>s.StopOrder).ToListAsync(ct)) {
                static string Csv(string v)=>"\""+v.Replace("\"","\"\"")+"\"";
                report.Add(string.Join(",",id,Csv(route.Label),BusinessTime.Utc(trip.DepartureUtc).ToString("O"),
                    BusinessTime.Utc(trip.ArrivalUtc).ToString("O"),trip.Fare.ToString(CultureInfo.InvariantCulture),stop.StopOrder,
                    Csv(stop.Name),TripStopService.AtUtc(trip,stop).ToString("O"),stop.IsIllustrative,locked?"locked-kept":action));
            }
            db.ChangeTracker.Clear();
        }
        await File.WriteAllLinesAsync(reportPath,report,System.Text.Encoding.UTF8,ct);
        Console.WriteLine($"Đã kiểm {ids.Count} chuyến. Không thay tuyến/xe/giờ/giá/phân công. Báo cáo: {reportPath}");
    }
    // Teaching examples only. Exact durations, no fabricated official timetable or arbitrary interpolation.
    public static (string Name,int Minutes)[] StudyTemplate(string origin,string destination,int duration) =>
        (origin,destination,duration) switch {
            ("TP. Hồ Chí Minh","Đà Lạt",420)=>[("Bảo Lộc",240),("Đức Trọng",330)],
            ("Đà Lạt","TP. Hồ Chí Minh",420)=>[("Đức Trọng",90),("Bảo Lộc",180)],
            ("TP. Hồ Chí Minh","Nha Trang",480)=>[("Cam Ranh",330),("Cam Lâm",400)],
            _=>[]
        };
}
