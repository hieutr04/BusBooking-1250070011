using System.Security.Claims;
using BusBooking.Data;
using BusBooking.Models;
using BusBooking.Services;
using BusBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Controllers;
[Authorize(Roles="Customer,Staff,Admin")]
public class BookingsController(AppDbContext db,BookingService service):Controller {
    private string Actor=>User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index(string? state,int page=1,CancellationToken ct=default) {
        state??="all";
        Guard.Input(ModelState.IsValid&&state is "all" or "upcoming" or "due" or "past" or "closed","Bộ lọc vé không hợp lệ.");
        var now=await new DbGate(db).Now(ct);
        var bookings=db.Bookings.AsNoTracking().Where(b=>b.OwnerId==Actor);
        var active=bookings.Where(b=>b.Status==BookingStatus.Active&&(b.PaymentExpiresUtc==null||b.PaymentExpiresUtc>now));
        var upcoming=active.Where(b=>b.Hold.Trip.DepartureUtc>now&&b.Hold.Trip.Status!=TripStatus.Completed);
        var filtered=state switch {
            "upcoming"=>upcoming.Where(b=>b.Total<=(b.Payments.Sum(p=>(decimal?)p.Amount)??0)),
            "due"=>upcoming.Where(b=>b.Total>(b.Payments.Sum(p=>(decimal?)p.Amount)??0)),
            "past"=>active.Where(b=>b.Hold.Trip.DepartureUtc<=now||b.Hold.Trip.Status==TripStatus.Completed),
            "closed"=>bookings.Where(b=>b.Status!=BookingStatus.Active||b.PaymentExpiresUtc<=now),
            _=>bookings
        };
        const int size=20;
        var pages=Math.Max(1,(int)Math.Ceiling(await filtered.CountAsync(ct)/(double)size));
        page=Math.Clamp(page,1,pages);
        ViewBag.TicketState=state;ViewBag.Page=page;ViewBag.PageCount=pages;ViewBag.Now=now;
        return View(await filtered.Include(b=>b.Hold).ThenInclude(h=>h.Trip).ThenInclude(t=>t.Route)
            .Include(b=>b.Payments).Include(b=>b.Tickets).AsSplitQuery()
            .OrderByDescending(b=>b.CreatedUtc).ThenByDescending(b=>b.Id).Skip((page-1)*size).Take(size).ToListAsync(ct));
    }
    public async Task<IActionResult> Details(Guid id,[FromServices] DbGate gate) {
        var b=Guard.Found(await db.Bookings.AsNoTracking().Include(x=>x.Hold).ThenInclude(x=>x.Trip).ThenInclude(x=>x.Route).Include(x=>x.Tickets).ThenInclude(x=>x.Seat).Include(x=>x.Payments).SingleOrDefaultAsync(x=>x.Id==id));
        if(b.OwnerId!=Actor&&!User.IsInRole("Admin")&&!User.IsInRole("Staff"))return Forbid();
        var now=await gate.Now();
        ViewBag.PaymentServerUtc=now;
        ViewBag.CanChange=b.Status==Models.BookingStatus.Active&&BookingService.IsBeforeDeparture(b.Hold.Trip,now);
        ViewBag.CanSimulate=User.IsInRole("Customer")&&b.OwnerId==Actor&&ViewBag.CanChange is true&&b.Total>b.Balance&&
            (b.PaymentExpiresUtc==null||b.PaymentExpiresUtc>now);
        if(b.ReplacesBookingId is Guid originalId) {
            var original=await db.Bookings.Where(o=>o.Id==originalId).Select(o=>new {
                o.Status,MayExchange=o.Status==BookingStatus.Active&&o.Hold.Trip.Status==TripStatus.Scheduled&&
                    o.Hold.Trip.DepartureUtc>now&&db.ChangeRequests.Any(r=>r.BookingId==originalId&&r.Kind==ChangeKind.Exchange&&r.Status==RequestStatus.Pending)
            }).SingleAsync();
            ViewBag.OriginalActive=original.MayExchange;
            if(ViewBag.CanSimulate is true&&!original.MayExchange&&original.Status!=BookingStatus.Exchanged) {
                ViewBag.CanSimulate=false;
                ViewBag.ExchangeUnavailable=true;
            }
        }
        ViewBag.Requests=await db.ChangeRequests.AsNoTracking().Where(x=>x.BookingId==id).OrderByDescending(x=>x.CreatedUtc).ToListAsync();
        ViewBag.Replacement=await db.Bookings.AsNoTracking().Where(x=>x.ReplacesBookingId==id&&x.OwnerId==b.OwnerId)
            .OrderByDescending(x=>x.CreatedUtc).ThenByDescending(x=>x.Id).FirstOrDefaultAsync();
        return View(b);
    }
    [HttpPost,Authorize(Roles="Customer")]
    public async Task<IActionResult> SimulatePayment(Guid id,[FromServices] IConfiguration configuration,CancellationToken ct) {
        Guard.Input(ModelState.IsValid&&id!=Guid.Empty,"Thông tin vé không hợp lệ.");
        try {
            Guard.Rule(!string.IsNullOrWhiteSpace(configuration["DemoPayment:Account"])&&
                !string.IsNullOrWhiteSpace(configuration["DemoPayment:Bank"]),"Chưa cấu hình thanh toán QR.");
            await service.SimulatePayment(Actor,id,ct);
            TempData["Notice"]="Đặt vé thành công! Bạn có thể xem vé điện tử và mã check-in bên dưới.";
            return RedirectToAction(nameof(Details),new{id});
        }catch(BusinessException error) when(error.Status==409) {
            TempData["BookingActionError"]=error.Message;
            return RedirectToAction(nameof(Details),new{id});
        }
    }
    [HttpPost] public async Task<IActionResult> Request(RequestForm f) {
        if(!ModelState.IsValid)throw new BusinessException(400,"Nhập loại yêu cầu hợp lệ và lý do từ 5 đến 500 ký tự.");
        try{
            await service.RequestChange(Actor,f);
            TempData["Notice"]="Đã gửi yêu cầu. Nhân viên sẽ kiểm tra và xử lý trực tiếp; vé vẫn hiệu lực trong lúc chờ.";
        }catch(BusinessException error) when(error.Status==409){
            TempData["BookingActionError"]=error.Message;
        }
        return RedirectToAction(nameof(Details),new{id=f.BookingId});
    }
}
