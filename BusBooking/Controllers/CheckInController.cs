using System.Security.Claims;
using BusBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Controllers;

[Authorize(Roles="Admin,Staff,Driver")]
public class CheckInController(TicketCheckIn checkIn):Controller {
    [HttpPost]
    public async Task<IActionResult> Record(int tripId,string? code,CancellationToken ct) {
        if(!ModelState.IsValid)return BadRequest();
        var driver=User.IsInRole("Driver");
        try {
            TempData["Notice"]=await checkIn.Record(tripId,code,User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                User.IsInRole("Admin"),driver,ct);
        }catch(BusinessException error) when(error.Status is 400 or 404 or 409){
            TempData["CheckInError"]=error.Message;
        }
        return driver?RedirectToAction("Details","Driver",new{id=tripId}):
            RedirectToAction("Passengers","Staff",new{id=tripId});
    }
}
