using BusBooking.Data;
using BusBooking.Models;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Services;

public sealed record StopAppendPreview(long Revision,StopAppendPlan Plan,bool CanAppend,string AppendReason,bool CanAddSpecific=false,string SpecificReason="");
public sealed record StopAppendResult(int TripId,long BeforeRevision,long AfterRevision,StopAppendPlan Plan,bool Applied);
public sealed class TripStopAppendService(AppDbContext db,DbGate gate,RealtimePublisher realtime) {
    public async Task<StopAppendPreview> Preview(int id,CancellationToken ct=default) {
        var trip=Guard.Found(await db.Trips.AsNoTracking().Include(x=>x.Route).SingleOrDefaultAsync(x=>x.Id==id,ct));
        var rows=await db.TripStops.AsNoTracking().Where(x=>x.TripId==id).OrderBy(x=>x.StopOrder).ToListAsync(ct);
        var now=await gate.Now(ct);var check=StopAppendPlanner.Check(trip,rows,now);
        var specific=SpecificStopPlanner.Check(trip,rows,now);
        return new(trip.Revision,StopAppendPlanner.Dalat(trip,trip.Route,rows,now),!check.Blocked,check.Message,specific.Ready,specific.Message);
    }
    public Task<StopAppendResult> CompleteDalat(string actor,int id,long? expectedRevision,bool dryRun=false,CancellationToken ct=default)=>
        Apply(actor,id,expectedRevision,null,dryRun,ct);
    public Task<StopAppendResult> Append(string actor,int id,long expectedRevision,StopAppendRequest request,CancellationToken ct=default)=>
        Apply(actor,id,expectedRevision,new[]{request},false,ct);
    public async Task<StopAppendResult> Specific(string actor,int id,long expectedRevision,IReadOnlyList<SpecificStopRequest> requests,bool dryRun=false,CancellationToken ct=default) {
        Trip trip;StopAppendPlan plan;long before;
        await using(var tx=await db.Database.BeginTransactionAsync(ct)) {
            trip=await gate.Trip(id,ct);before=trip.Revision;
            Guard.Rule(trip.Revision==expectedRevision,"Lịch vừa thay đổi. Tải lại và duyệt lại điểm trước khi cập nhật.");
            var route=await db.Routes.AsNoTracking().SingleAsync(x=>x.Id==trip.BusRouteId,ct);
            var rows=await db.TripStops.AsNoTracking().Where(x=>x.TripId==id).OrderBy(x=>x.StopOrder).ToListAsync(ct);
            var now=await gate.Now(ct);plan=SpecificStopPlanner.Plan(trip,route,rows,requests,now);
            if(plan.Ready&&!dryRun) {
                await Persist(db,gate,trip,plan,actor,now,ct);
                foreach(var point in plan.NewStops) {
                    var source=requests.First(x=>StopPresentation.Key(x.Name)==StopPresentation.Key(point.Name));
                    gate.Audit(actor,"SpecificStopReviewed",point.Id.ToString(),now,"trip="+id+"; area="+source.Area+"; operator-confirmed=true; schedule=configured-from-trip; illustrative=true");
                    // AuditEntry.Detail is nvarchar(500): keep the original reference in its own bounded entry.
                    gate.Audit(actor,"SpecificStopSource",point.Id.ToString(),now,source.SourceReference.Trim());
                }
                await db.SaveChangesAsync(ct);
            }
            await tx.CommitAsync(ct);
        }
        if(plan.Ready&&!dryRun)await realtime.Publish(trip);
        return new(id,before,trip.Revision,plan,plan.Ready&&!dryRun);
    }
    private async Task<StopAppendResult> Apply(string actor,int id,long? revision,IReadOnlyList<StopAppendRequest>? requests,bool dryRun,CancellationToken ct) {
        Trip trip;StopAppendPlan plan;long before;
        await using(var tx=await db.Database.BeginTransactionAsync(ct)) {
            trip=await gate.Trip(id,ct);before=trip.Revision;
            Guard.Rule(!revision.HasValue||trip.Revision==revision.Value,"Lịch vừa được cập nhật. Vui lòng tải lại trước khi bổ sung điểm.");
            var route=await db.Routes.AsNoTracking().SingleAsync(x=>x.Id==trip.BusRouteId,ct);
            var rows=await db.TripStops.AsNoTracking().Where(x=>x.TripId==id).OrderBy(x=>x.StopOrder).ToListAsync(ct);
            var now=await gate.Now(ct);
            plan=requests==null?StopAppendPlanner.Dalat(trip,route,rows,now):StopAppendPlanner.Plan(trip,route,rows,requests,now);
            if(plan.Ready&&!dryRun)await Persist(db,gate,trip,plan,actor,now,ct);
            await tx.CommitAsync(ct);
        }
        if(plan.Ready&&!dryRun)await realtime.Publish(trip);
        return new(id,before,trip.Revision,plan,plan.Ready&&!dryRun);
    }
    // Caller owns the same trip gate as Acquire/Confirm. Only INSERT stops + trip Revision + audit.
    internal static async Task Persist(AppDbContext db,DbGate gate,Trip trip,StopAppendPlan plan,string actor,DateTime now,CancellationToken ct) {
        Guard.Rule(db.Database.CurrentTransaction!=null,"Không thể lưu lịch lúc này. Vui lòng thử lại.");
        Guard.Rule(!db.ChangeTracker.Entries<TripStop>().Any(x=>x.State is EntityState.Modified or EntityState.Deleted),
            "Có thay đổi điểm cũ chưa hoàn tất. Vui lòng tải lại lịch trước khi bổ sung.");
        Guard.Input(plan.NewStops.All(x=>x.Id==0&&x.TripId==trip.Id),"Điểm mới không thuộc chuyến đang chọn.");
        db.TripStops.AddRange(plan.NewStops);trip.Revision++;
        gate.Audit(actor,"TripStopsAppended",trip.Id.ToString(),now,"insert-only; source=configured-stops; count="+plan.NewStops.Count);
        await db.SaveChangesAsync(ct);
    }
}
