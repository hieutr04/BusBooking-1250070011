using System.Collections.Concurrent;
using System.Diagnostics;
namespace BusBooking.Services;
// Diagnostic data only; never an ownership lock or booking source of truth.
public class RealtimeMetrics(ILogger<RealtimeMetrics> log) {
    private readonly ConcurrentDictionary<(int Trip,long Revision),long> sent=new();
    public void Mark(int trip,long revision) {
        var now=Stopwatch.GetTimestamp();
        foreach(var old in sent.Where(x=>Stopwatch.GetElapsedTime(x.Value,now).TotalSeconds>60))sent.TryRemove(old.Key,out _);
        if(sent.Count>=1024)sent.TryRemove(sent.MinBy(x=>x.Value).Key,out _);
        sent[(trip,revision)]=now;
    }
    public double? Ack(int trip,long revision) {
        if(!sent.TryGetValue((trip,revision),out var start))return null;
        var ms=Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        if(ms>60000)return null;
        log.LogInformation("RealtimeAck Trip={TripId} Revision={Revision} PostCommitObservedToAckMs={AckMs}",trip,revision,ms);
        return ms;
    }
}
