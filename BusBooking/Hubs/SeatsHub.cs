using BusBooking.Data;
using BusBooking.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Hubs;
// Public read-only trip invalidations. Private hold data is fetched via an authenticated HTTP snapshot.
public class SeatsHub(AppDbContext db,RealtimeMetrics metrics):Hub {
    public async Task JoinTrip(int tripId) {
        if(!await db.Trips.AnyAsync(x=>x.Id==tripId))throw new HubException("Chuyến không tồn tại.");
        var old=Context.Items.TryGetValue("trip",out var v)?v as string:null;
        if(old!=null)await Groups.RemoveFromGroupAsync(Context.ConnectionId,old);
        var group=$"trip:{tripId}";Context.Items["trip"]=group;
        await Groups.AddToGroupAsync(Context.ConnectionId,group);
    }
    public double? AckRevision(int tripId,long revision) {
        if(!Context.Items.TryGetValue("trip",out var group)||group as string!=$"trip:{tripId}")throw new HubException("Bạn chưa tham gia nhóm chuyến này.");
        return metrics.Ack(tripId,revision);
    }
    public Task LeaveTrip(int tripId)=>Groups.RemoveFromGroupAsync(Context.ConnectionId,$"trip:{tripId}");
}
