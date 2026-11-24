using System.Security.Claims;
using BusBooking.Services;
using BusBooking.ViewModels;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BusBooking.Controllers;
[Route("api/seats")]
public class SeatsApiController(BookingService service,IAntiforgery antiforgery):Controller {
    private string Actor=>User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    [HttpGet("{tripId:int}")] public async Task<IActionResult> Snapshot(int tripId,CancellationToken ct)=>Ok(await service.Snapshot(tripId,User.Identity?.IsAuthenticated==true?Actor:null,ct));
    [HttpGet("csrf")] public IActionResult Csrf()=>Ok(new{token=antiforgery.GetAndStoreTokens(HttpContext).RequestToken});
    [Authorize(Roles="Customer,Staff,Admin"),HttpPost("hold")] public async Task<IActionResult> Hold([FromBody] HoldForm f,CancellationToken ct) {
        if(!ModelState.IsValid)return BadRequest(new{message="Chuyến hoặc danh sách ghế không hợp lệ."});return Ok(await service.Acquire(Actor,f,ct));
    }
    [Authorize(Roles="Customer,Staff,Admin"),HttpPost("release")] public async Task<IActionResult> Release([FromBody]TokenForm f,CancellationToken ct) {
        if(!ModelState.IsValid||f.HoldId==Guid.Empty)return BadRequest(new{message="Mã giữ chỗ không hợp lệ."});await service.Release(Actor,f.HoldId,ct);return Ok(new{released=true});
    }
    [Authorize(Roles="Customer,Staff,Admin"),HttpPost("stops")]
    public async Task<IActionResult> Stops([FromBody]HoldStopsForm f,CancellationToken ct) {
        if(!ModelState.IsValid||f.HoldId==Guid.Empty)return BadRequest(new{message="Điểm đón/trả không hợp lệ."});
        return Ok(await service.UpdateStops(Actor,f,ct));
    }
    [Authorize(Roles="Customer,Staff,Admin"),HttpPost("confirm")] public async Task<IActionResult> Confirm([FromBody]ConfirmForm f,CancellationToken ct) {
        if(!ModelState.IsValid)return BadRequest(new{message="Vui lòng nhập đủ tên hành khách, người liên hệ và số điện thoại hợp lệ."});
        var id=await service.Confirm(Actor,User.IsInRole("Admin")||User.IsInRole("Staff"),f,ct);
        var url=Url.Action("Details","Bookings",new{id});
        return Ok(new{id,url,showTripConfirmation=User.IsInRole("Customer")});
    }
}
