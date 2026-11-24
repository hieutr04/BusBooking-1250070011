using System.Security.Claims;
using BusBooking.Data;
using BusBooking.Models;
using BusBooking.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Controllers;
public class TripsController(AppDbContext db,DbGate gate):Controller {
    public async Task<IActionResult> Index(string? origin,string? destination,DateTime? date,Guid? exchangeBookingId,
        int? departureHour,VehicleKind? vehicleKind,string? pickup,string? quick,CancellationToken ct) {
        Guard.Input(ModelState.IsValid&&departureHour is null or 0 or 6 or 12 or 18,"Bộ lọc tìm chuyến không hợp lệ.");
        var now=await gate.Now(ct);
        var today=BusinessTime.Vietnam(now).Date;
        var local=(date??today.AddDays(1)).Date;
        if(quick is "today" or "tomorrow" or "reverse") {
            if(quick=="reverse") {
                (origin,destination)=(destination,origin);
                pickup=null;
            } else {
                local=quick=="today"?today:today.AddDays(1);
            }
            return RedirectToAction(nameof(Index),new{origin,destination,date=local.ToString("yyyy-MM-dd"),
                exchangeBookingId,departureHour,vehicleKind,pickup});
        }

        Guard.Input(local>DateTime.MinValue.Date&&local<DateTime.MaxValue.Date,"Ngày tìm kiếm không hợp lệ.");
        var start=BusinessTime.FromVietnam(local).AddHours(departureHour??0);
        var end=start.AddHours(departureHour.HasValue?6:24);
        var q=db.Trips.AsNoTracking().Where(x=>x.DepartureUtc>=start&&x.DepartureUtc<end&&x.DepartureUtc>now&&x.Status==TripStatus.Scheduled&&x.SalesOpen);
        if(!string.IsNullOrWhiteSpace(origin)){
            var places=ProvinceSearch.Match(origin);
            q=q.Where(x=>places.Contains(x.Route.Origin));
        }
        if(!string.IsNullOrWhiteSpace(destination)){
            var places=ProvinceSearch.Match(destination);
            q=q.Where(x=>places.Contains(x.Route.Destination));
        }
        if(vehicleKind.HasValue)q=q.Where(t=>t.Vehicle.Kind==vehicleKind);

        var candidates=await q.Include(t=>t.Route).Include(t=>t.Vehicle)
            .OrderBy(t=>t.DepartureUtc).ThenBy(t=>t.Id).ToListAsync(ct);
        var candidateIds=q.Select(t=>t.Id);
        var stops=(await db.TripStops.AsNoTracking().Where(s=>s.IsActive&&candidateIds.Contains(s.TripId))
            .ToListAsync(ct)).ToLookup(s=>s.TripId);
        var matchingStops=new Dictionary<int,TripStop>();
        var pickupNames=new Dictionary<string,string>();
        var pickupKey=StopPresentation.Key(pickup);
        foreach(var trip in candidates) {
            var allowed=StopChoicePolicy.ForNew(trip,trip.Route,stops[trip.Id].ToArray());
            var lastDropoff=allowed.LastOrDefault(s=>s.CanDropoff);
            if(lastDropoff==null)continue;
            foreach(var stop in allowed.Where(s=>s.CanPickup&&s.StopOrder<lastDropoff.StopOrder&&
                TripStopService.AtUtc(trip,s)<TripStopService.AtUtc(trip,lastDropoff))) {
                var name=StopPresentation.Name(stop.Name);
                var key=StopPresentation.Key(name);
                if(key.Length==0)continue;
                pickupNames.TryAdd(key,name);
                if(key==pickupKey)matchingStops.TryAdd(trip.Id,stop);
            }
        }
        if(pickupKey.Length>0)pickupNames.TryAdd(pickupKey,pickup!.Trim());
        var matches=pickupKey.Length==0?candidates:candidates.Where(t=>matchingStops.ContainsKey(t.Id)).ToList();
        var result=matches.Take(100).ToList();

        ViewBag.Routes=await db.Routes.AsNoTracking().Where(x=>x.IsActive).ToListAsync(ct);
        ViewBag.Origin=origin;ViewBag.Destination=destination;ViewBag.Date=local;ViewBag.ExchangeBookingId=exchangeBookingId;
        ViewBag.DepartureHour=departureHour;ViewBag.VehicleKind=vehicleKind;
        ViewBag.PickupKey=pickupKey;ViewBag.PickupOptions=pickupNames.OrderBy(x=>x.Value).ToList();
        ViewBag.MatchingStops=matchingStops;ViewBag.TotalMatches=matches.Count;
        var ids=result.Select(x=>x.Id).ToArray();
        ViewBag.Available=await db.TripSeats.Where(x=>ids.Contains(x.TripId)&&(x.Status==SeatStatus.Available||x.Status==SeatStatus.Held&&x.CurrentHold!.ExpiresAtUtc<=now)).GroupBy(x=>x.TripId).Select(g=>new{TripId=g.Key,Count=g.Count()}).ToDictionaryAsync(x=>x.TripId,x=>x.Count,ct);
        return View(result);
    }
    [ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
    public async Task<IActionResult> Details(int id,Guid? exchangeBookingId=null,Guid? confirmedBookingId=null,int? pickupStopId=null) {
        var t=Guard.Found(await db.Trips.AsNoTracking().Include(x=>x.Route).Include(x=>x.Vehicle).SingleOrDefaultAsync(x=>x.Id==id));
        if(confirmedBookingId.HasValue){
            if(!User.IsInRole("Customer"))return Forbid();
            var actor=User.FindFirstValue(ClaimTypes.NameIdentifier);
            var confirmed=await db.Bookings.AsNoTracking().Include(x=>x.Payments).Include(x=>x.Hold)
                .SingleOrDefaultAsync(x=>x.Id==confirmedBookingId&&x.OwnerId==actor&&x.Hold.TripId==id);
            if(confirmed is null)return NotFound();
            var paymentNow=await gate.Now();
            ViewBag.PaymentServerUtc=paymentNow;
            ViewBag.ConfirmedBooking=confirmed;
            ViewBag.CanSimulate=confirmed.Status==Models.BookingStatus.Active&&confirmed.Total>confirmed.Balance&&
                BookingService.IsBeforeDeparture(t,paymentNow)&&(confirmed.PaymentExpiresUtc==null||confirmed.PaymentExpiresUtc>paymentNow);
        }
        ViewBag.PreferredPickupStopId=pickupStopId;
        ViewBag.ExchangeBookingId=exchangeBookingId;return View(t);
    }
}
