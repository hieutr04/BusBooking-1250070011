using System.Security.Claims;
using BusBooking.Data;
using BusBooking.Models;
using BusBooking.Services;
using BusBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Controllers;
[Authorize(Roles="Staff,Admin")]
public class StaffController(AppDbContext db,BookingService service):Controller {
    private string Actor=>User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index(string? q,string? payment) {
        var query=db.Bookings.AsNoTracking().Include(x=>x.Payments).Include(x=>x.Tickets).Include(x=>x.Hold).ThenInclude(x=>x.Trip).ThenInclude(x=>x.Route).AsQueryable();
        if(!string.IsNullOrWhiteSpace(q))query=query.Where(x=>x.Code.Contains(q)||x.ContactPhone.Contains(q)||x.ContactName.Contains(q));
        query=payment switch {
            "due"=>query.Where(x=>x.Status==Models.BookingStatus.Active&&(x.Payments.Sum(p=>(decimal?)p.Amount)??0)<x.Total),
            "paid"=>query.Where(x=>x.Status==Models.BookingStatus.Active&&(x.Payments.Sum(p=>(decimal?)p.Amount)??0)>=x.Total),
            "refund"=>query.Where(x=>x.Status!=Models.BookingStatus.Active&&(x.Payments.Sum(p=>(decimal?)p.Amount)??0)>0),
            _=>query
        };
        ViewBag.Query=q;ViewBag.Payment=payment;
        return View(await query.OrderByDescending(x=>x.CreatedUtc).ThenBy(x=>x.Id).Take(100).ToListAsync());
    }
    public IActionResult Sell()=>RedirectToAction("Index","Trips");
    [HttpPost] public async Task<IActionResult> Money(PaymentForm f) {
        if(!ModelState.IsValid||!f.Refund)throw new BusinessException(400,"Chỉ hỗ trợ ghi nhận hoàn tiền tại đây. Khách hoàn tất thanh toán tại trang QR.");
        await service.Money(Actor,f);
        TempData["Notice"]="Đã ghi nhận hoàn tiền.";
        return RedirectToAction("Details","Bookings",new{id=f.BookingId});
    }
    [HttpPost]
    public async Task<IActionResult> Cancel(Guid id) {
        try{
            await service.Cancel(Actor,id);
            TempData["Notice"]="Đã hủy vé và nhả ghế; khoản đã thanh toán được chuyển sang chờ xử lý hoàn tiền.";
        }catch(BusinessException error) when(error.Status==409){
            TempData["BookingActionError"]=error.Message;
        }
        return RedirectToAction("Details","Bookings",new{id});
    }
    public async Task<IActionResult> Requests()=>View(await db.ChangeRequests.AsNoTracking().Include(x=>x.Booking).OrderBy(x=>x.Status).ThenByDescending(x=>x.CreatedUtc).Take(200).ToListAsync());
    [HttpPost] public async Task<IActionResult> Reject(Guid id,string resolution){await service.RejectRequest(Actor,id,resolution);return RedirectToAction(nameof(Requests));}
    public async Task<IActionResult> Trips(DateTime? date,int? routeId,int? vehicleId,string? driverId,int page=1,CancellationToken ct=default) {
        Guard.Input(ModelState.IsValid,"Bộ lọc lịch chuyến không hợp lệ.");
        return View(await TripSchedule.Load(db,date,routeId,vehicleId,driverId,page,ct));
    }
    public async Task<IActionResult> Passengers(int id) {
        ViewBag.Trip=Guard.Found(await db.Trips.AsNoTracking().Include(x=>x.Route).SingleOrDefaultAsync(x=>x.Id==id));
        return View("~/Views/Shared/Passengers.cshtml",await PassengerList(id,HttpContext.RequestAborted));
    }
    private Task<List<Ticket>> PassengerList(int id,CancellationToken ct)=>
        db.Tickets.AsNoTracking().Include(x=>x.Seat).Include(x=>x.Booking)
            .Where(x=>x.Seat.TripId==id&&x.IsActive&&x.Booking.PaymentExpiresUtc==null)
            .OrderBy(x=>x.Seat.Code).ToListAsync(ct);

    [HttpGet,ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
    public async Task<IActionResult> ExportPassengers(int id,CancellationToken ct) {
        Guard.Found(await db.Trips.AsNoTracking().SingleOrDefaultAsync(t=>t.Id==id,ct));
        var tickets=await PassengerList(id,ct);
        var rows=new List<string[]>{new[]{"Ghế","Hành khách","Mã vé","Điểm đón","Giờ đón (VN)","Trạng thái check-in","Giờ check-in (VN)"}};
        foreach(var t in tickets)
            rows.Add([t.Seat.Code,t.PassengerName,t.Booking.Code,StopPresentation.Name(t.Booking.PickupName),
                t.Booking.PickupAtUtc is DateTime pickup?BusinessTime.Text(pickup):"",
                t.CheckedInUtc.HasValue?"Đã lên xe":"Chưa lên xe",
                t.CheckedInUtc is DateTime checkedIn?BusinessTime.Text(checkedIn):""]);
        return File(CsvDownload.Encode(rows),"text/csv; charset=utf-8",$"hanh-khach-chuyen-{id}.csv");
    }
}
