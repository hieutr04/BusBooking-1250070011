using System.Security.Claims;
using BusBooking.Data;
using BusBooking.Models;
using BusBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Controllers;
[Authorize(Roles="Driver")]
public class DriverController(AppDbContext db,BookingService service):Controller {
    private string Actor=>User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index()=>View(await db.Trips.AsNoTracking().Include(x=>x.Route).Include(x=>x.Vehicle).Where(x=>x.DriverId==Actor||x.CoDriverId==Actor).OrderBy(x=>x.DepartureUtc).Take(100).ToListAsync());
    public async Task<IActionResult> Details(int id) {
        var trip=Guard.Found(await db.Trips.AsNoTracking().Include(x=>x.Route).SingleOrDefaultAsync(x=>x.Id==id));
        if(trip.DriverId!=Actor&&trip.CoDriverId!=Actor)return Forbid();ViewBag.Trip=trip;
        return View("~/Views/Shared/Passengers.cshtml",await db.Tickets.AsNoTracking().Include(x=>x.Seat).Include(x=>x.Booking).Where(x=>x.Seat.TripId==id&&x.IsActive&&x.Booking.PaymentExpiresUtc==null).OrderBy(x=>x.Seat.Code).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Progress(int id,TripStatus next) {
        if(!ModelState.IsValid)throw new BusinessException(400,"Trạng thái không hợp lệ.");await service.SetTripState(Actor,false,id,next,null);return RedirectToAction(nameof(Details),new{id});
    }
}
