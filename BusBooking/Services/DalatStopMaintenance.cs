using System.Globalization;
using System.Text;
using BusBooking.Data;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Services;

public static class DalatStopMaintenance {
    public static async Task<bool> Run(AppDbContext db,TripStopAppendService service,bool dryRun,string reportPath,CancellationToken ct=default) {
        if((await db.Database.GetPendingMigrationsAsync(ct)).Any())
            throw new InvalidOperationException("Áp dụng đúng migration còn thiếu bằng tài khoản triển khai trước khi cập nhật điểm. Không chạy lại Initial hoặc seed.");
        var trips=await db.Trips.AsNoTracking().Select(t=>new {t.Id,t.Route.Origin,t.Route.Destination}).OrderBy(t=>t.Id).ToListAsync(ct);
        var ids=trips.Where(t=>StopPresentation.Key(t.Origin)=="da lat"&&StopPresentation.Key(t.Destination)=="ho chi minh").Select(t=>t.Id).ToArray();
        var lines=new List<string>{"TripId,BeforeRevision,AfterRevision,Status,Reason,StopId,StopOrder,StopName,OffsetMinutes,AtUtc,AtVietnam,IsIllustrative"};
        var added=0;var complete=0;var blocked=0;
        static string Csv(string value)=>"\""+value.Replace("\"","\"\"")+"\"";
        foreach(var id in ids) {
            var result=await service.CompleteDalat("command",id,null,dryRun,ct);
            var status=result.Applied?"applied":result.Plan.Ready?"planned":result.Plan.Status;
            if(result.Applied||result.Plan.Ready)added++;else if(result.Plan.Status=="complete")complete++;else blocked++;
            var trip=await db.Trips.AsNoTracking().SingleAsync(t=>t.Id==id,ct);
            var rows=await db.TripStops.AsNoTracking().Where(x=>x.TripId==id).OrderBy(x=>x.StopOrder).ToListAsync(ct);
            if(dryRun&&result.Plan.Ready)rows=rows.Concat(result.Plan.NewStops).OrderBy(x=>x.StopOrder).ToList();
            if(rows.Count==0)lines.Add(string.Join(",",id,result.BeforeRevision,result.AfterRevision,status,Csv(result.Plan.Message),"","","","","","",""));
            foreach(var row in rows)lines.Add(string.Join(",",id,result.BeforeRevision,result.AfterRevision,status,Csv(result.Plan.Message),row.Id,row.StopOrder,
                Csv(row.Name),row.OffsetMinutes,TripStopService.AtUtc(trip,row).ToString("O",CultureInfo.InvariantCulture),
                Csv(BusinessTime.Text(TripStopService.AtUtc(trip,row))),row.IsIllustrative));
            db.ChangeTracker.Clear();
        }
        // File IO is outside every SQL transaction. No contact/passenger/hold tokens are exported.
        await File.WriteAllLinesAsync(reportPath,lines,Encoding.UTF8,ct);
        Console.WriteLine($"{(dryRun?"Dự kiến bổ sung":"Đã bổ sung")}: {added} chuyến; đã đầy đủ: {complete}; cần kiểm tra/không thay đổi: {blocked}. Báo cáo: {Path.GetFullPath(reportPath)}");
        return blocked==0;
    }
}
