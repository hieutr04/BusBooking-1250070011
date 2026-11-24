using BusBooking.Data;
using BusBooking.Models;
using BusBooking.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace BusBooking.BackgroundServices;
public class HoldExpiryWorker(IServiceScopeFactory scopes,IOptions<BookingOptions> config,ILogger<HoldExpiryWorker> log):BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        using var timer=new PeriodicTimer(TimeSpan.FromSeconds(config.Value.ExpiryScanSeconds));
        do {
            try {
                using var scope=scopes.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();var gate=scope.ServiceProvider.GetRequiredService<DbGate>();
                var now=await gate.Now(stoppingToken);
                // bounded indexed batch; idempotent conditional release under the shared trip gate
                var trips=await db.SeatHolds.AsNoTracking().Where(x=>x.Status==HoldStatus.Active&&x.ExpiresAtUtc<=now).OrderBy(x=>x.ExpiresAtUtc).Take(config.Value.ExpiryBatchSize).Select(x=>x.TripId).ToListAsync(stoppingToken);
                var payments=await db.Bookings.AsNoTracking().Where(x=>x.Status==BookingStatus.Active&&x.PaymentExpiresUtc<=now)
                    .OrderBy(x=>x.PaymentExpiresUtc).Take(config.Value.ExpiryBatchSize).Select(x=>x.Hold.TripId).ToListAsync(stoppingToken);
                foreach(var id in trips.Union(payments)) {
                    using var work=scopes.CreateScope();await work.ServiceProvider.GetRequiredService<BookingService>().ExpireTrip(id,stoppingToken);
                }
            } catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested) {break;}
            catch(Exception e) {log.LogError(e,"Hold or payment expiry scan failed. API checks still enforce SQL expiry. Retrying next scan.");}
        } while(await timer.WaitForNextTickAsync(stoppingToken));
    }
}
