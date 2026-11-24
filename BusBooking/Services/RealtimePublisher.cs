using BusBooking.Hubs;
using BusBooking.Models;
using BusBooking.ViewModels;
using Microsoft.AspNetCore.SignalR;
namespace BusBooking.Services;
public class RealtimePublisher(IHubContext<SeatsHub> hub,ILogger<RealtimePublisher> log,RealtimeMetrics metrics) {
    public async Task Publish(Trip trip) {
        // Invalidation only: no ownership, tokens or passenger data in the public group.
        metrics.Mark(trip.Id,trip.Revision);
        try {await hub.Clients.Group($"trip:{trip.Id}").SendAsync("TripChanged",new TripEvent(trip.Id,trip.Revision,DateTime.UtcNow));}
        catch(Exception e) {log.LogError(e,"SQL committed; SignalR delivery failed for trip {TripId} revision {Revision}. Clients must resync.",trip.Id,trip.Revision);}
    }
}
