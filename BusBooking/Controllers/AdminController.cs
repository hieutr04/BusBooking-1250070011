using System.Security.Claims;
using BusBooking.Data;
using BusBooking.Models;
using BusBooking.Services;
using BusBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Controllers;
[Authorize(Roles="Admin")]
public class AdminController(AppDbContext db,CatalogService catalog,BookingService booking,ReportService reports,UserManager<AppUser> users,TripStopAppendService stopAppend):Controller {
    private string Actor=>User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index(DateTime? from,DateTime? to,string period="custom",DateTime? on=null,
        int? year=null,int? month=null,int? quarter=null,CancellationToken ct=default) {
        Guard.Input(ModelState.IsValid,"Kỳ báo cáo không hợp lệ.");
        Guard.Input(period is "custom" or "day" or "month" or "quarter" or "year","Kỳ báo cáo không hợp lệ.");
        var today=BusinessTime.Vietnam(DateTime.UtcNow).Date;
        if(period=="day"){
            from=to=(on??today).Date;
        }else if(period!="custom"){
            var selectedYear=year??today.Year;
            var selectedMonth=month??today.Month;
            var selectedQuarter=quarter??((today.Month-1)/3+1);
            Guard.Input(selectedYear is >=1900 and <=9998,"Năm báo cáo không hợp lệ.");
            Guard.Input(period!="month"||selectedMonth is >=1 and <=12,"Tháng báo cáo không hợp lệ.");
            Guard.Input(period!="quarter"||selectedQuarter is >=1 and <=4,"Quý báo cáo không hợp lệ.");
            var startMonth=period=="month"?selectedMonth:period=="quarter"?(selectedQuarter-1)*3+1:1;
            var start=new DateTime(selectedYear,startMonth,1);
            from=start;
            to=start.AddMonths(period=="month"?1:period=="quarter"?3:12).AddDays(-1);
        }
        ViewBag.ReportPeriod=period;
        return View(await reports.Dashboard(from,to,ct,overviewPeriod:period));
    }
    [HttpGet]
    public async Task<IActionResult> ExportRevenue(DateTime? from,DateTime? to,CancellationToken ct) {
        Guard.Input(ModelState.IsValid,"Kỳ báo cáo không hợp lệ.");
        var report=await reports.Dashboard(from,to,ct);
        var rows=new List<object[]> {new[]{"Từ ngày","Đến ngày","Tuyến xe","Thu (VND)","Hoàn (VND)","Thu ròng (VND)"}};
        object[] Row(string route,decimal received,decimal refunded)=>[
            report.From.ToString("yyyy-MM-dd"),report.To.ToString("yyyy-MM-dd"),route,
            received,refunded,received-refunded
        ];
        rows.AddRange(report.Revenue.OrderBy(r=>r.Label).Select(r=>Row(r.Label,r.Receipts,r.Refunds)));
        rows.Add(Row("Tổng cộng",report.Receipts,report.Refunds));
        return File(CsvDownload.Encode(rows),"text/csv; charset=utf-8",$"doanh-thu-{report.From:yyyyMMdd}-{report.To:yyyyMMdd}.csv");
    }
    [HttpGet]
    public async Task<IActionResult> HoldMonitor(int? id,CancellationToken ct) {
        var trips=await db.Trips.AsNoTracking().Include(x=>x.Route)
            .OrderBy(x=>x.Status!=TripStatus.Scheduled).ThenByDescending(x=>x.DepartureUtc).Take(100).ToListAsync(ct);
        if(id.HasValue&&!trips.Any(t=>t.Id==id)){
            trips.Insert(0,Guard.Found(await db.Trips.AsNoTracking().Include(x=>x.Route).SingleOrDefaultAsync(t=>t.Id==id,ct)));
        }
        ViewBag.TripId=id??trips.FirstOrDefault()?.Id;
        return View(trips);
    }
    [HttpGet]
    [ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
    public async Task<IActionResult> HoldMonitorData(int id,[FromServices] DbGate gate,CancellationToken ct) {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var trip=await gate.Trip(id,ct);
        var now=await gate.Now(ct);
        var rows=await db.TripSeats.AsNoTracking().Where(s=>s.TripId==id)
            .Select(s=>new{s.Id,s.Code,s.Row,s.Column,s.Floor,s.Status,s.CurrentHoldId,
                Expires=s.CurrentHold==null?(DateTime?)null:s.CurrentHold.ExpiresAtUtc}).ToListAsync(ct);
        var holds=rows.Where(s=>s.Status==SeatStatus.Held&&s.CurrentHoldId.HasValue&&s.Expires>now)
            .GroupBy(s=>new{s.CurrentHoldId,s.Expires}).OrderBy(g=>g.Key.Expires)
            .Select(g=>new{expiresAtUtc=BusinessTime.Utc(g.Key.Expires!.Value),codes=g.Select(s=>s.Code).Order().ToArray()}).ToArray();
        var expiredSeats=await db.HoldSeats.CountAsync(s=>s.TripId==id&&s.Hold.Status==HoldStatus.Expired,ct);
        var seats=rows.Select(s=>new{s.Id,s.Code,s.Row,s.Column,s.Floor,
            status=s.Status==SeatStatus.Held&&s.Expires<=now?"Available":s.Status.ToString()}).ToArray();
        await tx.CommitAsync(ct);
        return Ok(new{serverUtc=now,trip.Revision,holds,seats,expiredSeats});
    }
    public async Task<IActionResult> Routes()=>View(await db.Routes.AsNoTracking().OrderBy(x=>x.Origin).ToListAsync());
    [HttpGet] public async Task<IActionResult> RouteEdit(int id=0) {
        if(id==0)return View(new RouteForm());var x=Guard.Found(await db.Routes.FindAsync(id));return View(new RouteForm{Id=x.Id,Origin=x.Origin,Destination=x.Destination,DurationMinutes=x.DurationMinutes,Description=StopPresentation.Description(x.Description)??"",IsActive=x.IsActive});
    }
    [HttpPost] public async Task<IActionResult> RouteEdit(RouteForm f) {
        if(!ModelState.IsValid){Response.StatusCode=400;return View(f);}
        // Empty presentation of the known internal marker must not erase its DB provenance.
        if(f.Id>0&&string.IsNullOrWhiteSpace(f.Description)) {
            var stored=await db.Routes.AsNoTracking().Where(x=>x.Id==f.Id).Select(x=>x.Description).SingleOrDefaultAsync();
            if(StopPresentation.InternalDescription(stored))f.Description=stored!;
        }
        await catalog.SaveRoute(Actor,f);return RedirectToAction(nameof(Routes));
    }
    public async Task<IActionResult> Layouts()=>View(await db.Layouts.AsNoTracking().Include(x=>x.Seats).ToListAsync());
    [HttpGet] public async Task<IActionResult> LayoutEdit(int id=0) {
        if(id==0)return View(new LayoutForm());var x=Guard.Found(await db.Layouts.FindAsync(id));return View(new LayoutForm{Id=x.Id,Name=x.Name,Rows=x.Rows,Columns=x.Columns,Floors=x.Floors,IsActive=x.IsActive});
    }
    [HttpPost] public async Task<IActionResult> LayoutEdit(LayoutForm f) {if(!ModelState.IsValid){Response.StatusCode=400;return View(f);}await catalog.SaveLayout(Actor,f);return RedirectToAction(nameof(Layouts));}
    public async Task<IActionResult> Drivers() {
        var drivers=await users.GetUsersInRoleAsync(Roles.Driver);
        return View(drivers.OrderByDescending(x=>x.IsActive).ThenBy(x=>x.FullName).ThenBy(x=>x.Email).ToList());
    }
    [HttpGet] public async Task<IActionResult> DriverEdit(string? id=null) {
        if(string.IsNullOrEmpty(id))return View(new DriverForm());
        var driver=Guard.Found(await users.FindByIdAsync(id));
        if(!await users.IsInRoleAsync(driver,Roles.Driver))return NotFound();
        return View(new DriverForm {
            Id=driver.Id, FullName=driver.FullName, Email=driver.Email??"", PhoneNumber=driver.PhoneNumber,
            DateOfBirth=driver.DateOfBirth, IsActive=driver.IsActive
        });
    }
    [HttpPost] public async Task<IActionResult> DriverEdit(DriverForm f,CancellationToken ct) {
        if(ModelState.IsValid) {
            try {
                await catalog.SaveDriver(Actor,f,ct);
                TempData["Notice"]="Đã lưu hồ sơ tài xế.";
                return RedirectToAction(nameof(Drivers));
            }catch(BusinessException e) when(e.Status is 400 or 409) {
                ModelState.AddModelError("",e.Message);
                Response.StatusCode=e.Status;
            }
        }else Response.StatusCode=400;
        return View(f);
    }
    private async Task<AppUser?> PasswordTarget(string? id) {
        if(string.IsNullOrWhiteSpace(id))return null;
        var driver=await users.Users.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id);
        return driver is not null&&await users.IsInRoleAsync(driver,Roles.Driver)&&!await users.IsInRoleAsync(driver,Roles.Admin)
            ? driver : null;
    }
    private async Task<IActionResult> DriverPasswordView(AppUser driver,NewPasswordForm form) {
        ViewData["DriverId"]=driver.Id;
        ViewData["DriverName"]=driver.FullName;
        ViewData["DriverEmail"]=driver.Email;
        ViewData["HasPassword"]=await users.HasPasswordAsync(driver);
        return View("DriverPassword",form);
    }
    [HttpGet] public async Task<IActionResult> DriverPassword(string? id) {
        var driver=await PasswordTarget(id);
        if(driver is null)return NotFound();
        return await DriverPasswordView(driver,new NewPasswordForm());
    }
    [HttpPost] public async Task<IActionResult> DriverPassword(string? id,NewPasswordForm form,CancellationToken ct) {
        var driver=await PasswordTarget(id);
        if(driver is null)return NotFound();
        if(ModelState.IsValid) {
            try {
                await catalog.ResetDriverPassword(Actor,driver.Id,form.NewPassword,ct);
                TempData["Notice"]="Đã cập nhật mật khẩu cho tài xế.";
                return RedirectToAction(nameof(DriverEdit),new{id=driver.Id});
            }catch(BusinessException error) when(error.Status is 400 or 409) {
                ModelState.AddModelError("",error.Message);
                Response.StatusCode=error.Status;
            }
        }else Response.StatusCode=400;
        return await DriverPasswordView(driver,form);
    }
    public async Task<IActionResult> Vehicles()=>View(await db.Vehicles.AsNoTracking().Include(x=>x.Layout).OrderBy(x=>x.Plate).ToListAsync());
    private async Task VehicleOptions() {
        ViewBag.Layouts=new SelectList(await db.Layouts.AsNoTracking().OrderBy(x=>x.Name).ToListAsync(),"Id","Name");
    }
    [HttpGet] public async Task<IActionResult> VehicleEdit(int id=0) {
        await VehicleOptions();
        if(id==0)return View(new VehicleForm());
        var x=Guard.Found(await db.Vehicles.FindAsync(id));
        return View(new VehicleForm{Id=x.Id,Name=x.Name,Plate=x.Plate,Kind=x.Kind,SeatLayoutId=x.SeatLayoutId,IsActive=x.IsActive});
    }
    [HttpPost] public async Task<IActionResult> VehicleEdit(VehicleForm f,CancellationToken ct) {
        if(ModelState.IsValid) {
            try {
                await catalog.SaveVehicle(Actor,f,ct);
                TempData["Notice"]="Đã lưu thông tin xe.";
                return RedirectToAction(nameof(Vehicles));
            }catch(BusinessException e) when(e.Status is 400 or 409) {
                ModelState.AddModelError("",e.Message);
                Response.StatusCode=e.Status;
            }
        }else Response.StatusCode=400;
        await VehicleOptions();
        return View(f);
    }
    public async Task<IActionResult> Trips(DateTime? date,int? routeId,int? vehicleId,string? driverId,int page=1,CancellationToken ct=default) {
        Guard.Input(ModelState.IsValid,"Bộ lọc lịch chuyến không hợp lệ.");
        return View(await TripSchedule.Load(db,date,routeId,vehicleId,driverId,page,ct));
    }
    private async Task TripOptions() {
        ViewBag.Routes=new SelectList((await db.Routes.AsNoTracking().ToListAsync()).Select(x=>new{ x.Id,Name=x.Label}),"Id","Name");
        ViewBag.Vehicles=new SelectList(await db.Vehicles.AsNoTracking().ToListAsync(),"Id","Plate");
        ViewBag.Drivers=new SelectList((await users.GetUsersInRoleAsync("Driver")).Where(x=>x.IsActive),"Id","FullName");
        ViewBag.Staffs=new SelectList((await users.GetUsersInRoleAsync("Staff")).Where(x=>x.IsActive),"Id","FullName");
    }
    [HttpGet]
    public async Task<IActionResult> TripBatch() {
        await TripOptions();
        return View(new TripBatchForm());
    }
    [HttpPost]
    public async Task<IActionResult> TripBatch(TripBatchForm form,string operation,CancellationToken ct) {
        if(ModelState.IsValid&&operation is "preview" or "create") {
            try {
                ViewBag.BatchRows=await catalog.BatchTrips(Actor,form,operation=="create",ct);
                ViewBag.BatchCreated=operation=="create";
            }catch(BusinessException error) when(error.Status is 400 or 404 or 409) {
                ModelState.AddModelError("",error.Message);
                Response.StatusCode=error.Status;
            }
        }else {
            Response.StatusCode=400;
        }
        await TripOptions();
        return View(form);
    }
    [HttpGet] public async Task<IActionResult> TripEdit(int id=0) {
        await TripOptions();if(id==0)return View(new TripForm());var x=Guard.Found(await db.Trips.FindAsync(id));return View(new TripForm{Id=x.Id,BusRouteId=x.BusRouteId,VehicleId=x.VehicleId,DepartureLocal=BusinessTime.Vietnam(x.DepartureUtc),ArrivalLocal=BusinessTime.Vietnam(x.ArrivalUtc),Fare=x.Fare,DriverId=x.DriverId,CoDriverId=x.CoDriverId,StaffId=x.StaffId});
    }
    [HttpPost] public async Task<IActionResult> TripEdit(TripForm f) {if(!ModelState.IsValid){await TripOptions();Response.StatusCode=400;return View(f);}await catalog.SaveTrip(Actor,f);return RedirectToAction(nameof(Trips),new{date=f.DepartureLocal.ToString("yyyy-MM-dd")});}
    [HttpPost] public async Task<IActionResult> Sales(int id,bool open,string? date) {Guard.Input(ModelState.IsValid&&id>0,"Thông tin mở/đóng bán không hợp lệ.");await booking.SetTripState(Actor,true,id,null,open);return RedirectToAction(nameof(Trips),new{date});}
    [HttpPost] public async Task<IActionResult> CancelTrip(int id,string? date) {Guard.Input(ModelState.IsValid&&id>0,"Chuyến không hợp lệ.");await booking.SetTripState(Actor,true,id,TripStatus.Cancelled,null);return RedirectToAction(nameof(Trips),new{date});}
    private async Task<IActionResult> StopEditor(int id,int status,CancellationToken ct,StopAppendForm? draft=null) {
        var form=await new TripStopService(db,new DbGate(db)).Edit(id,ct);
        ViewBag.AppendPreview=await stopAppend.Preview(id,ct);ViewBag.AppendForm=draft;
        Response.StatusCode=status;return View("TripStops",form);
    }
    [HttpGet] public Task<IActionResult> TripStops(int id,CancellationToken ct)=>StopEditor(id,200,ct);
    [HttpPost] public async Task<IActionResult> TripStops(TripStopsForm f,CancellationToken ct) {
        var service=new TripStopService(db,new DbGate(db));
        if(!ModelState.IsValid){var current=await service.Edit(f.TripId,ct);f.RouteLabel=current.RouteLabel;f.DepartureUtc=current.DepartureUtc;f.ArrivalUtc=current.ArrivalUtc;f.Locked=current.Locked;ViewBag.AppendPreview=await stopAppend.Preview(f.TripId,ct);Response.StatusCode=400;return View(f);}
        await service.Save(Actor,f,ct);TempData["Notice"]="Đã lưu lịch điểm đón/trả. Giờ toàn chuyến giữ nguyên.";
        return RedirectToAction(nameof(TripStops),new{id=f.TripId});
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> CompleteDalatStops(int id,long revision,CancellationToken ct) {
        Guard.Input(ModelState.IsValid&&id>0&&revision>=0,"Yêu cầu cập nhật lịch không hợp lệ.");
        try {
            var result=await stopAppend.CompleteDalat(Actor,id,revision,false,ct);
            if(result.Plan.Blocked||result.Plan.Status=="not-applicable") {
                ModelState.AddModelError("",result.Plan.Message);return await StopEditor(id,409,ct);
            }
            TempData["Notice"]=result.Applied?$"Đã bổ sung {result.Plan.NewStops.Count} điểm. Các điểm và thông tin đặt vé hiện có được giữ nguyên.":result.Plan.Message;
            return RedirectToAction(nameof(TripStops),new{id});
        }catch(BusinessException e){ModelState.AddModelError("",e.Message);return await StopEditor(id,e.Status,ct);}
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> AppendTripStop(StopAppendForm f,CancellationToken ct) {
        Guard.Input(f.TripId>0,"Chuyến không hợp lệ.");
        if(!ModelState.IsValid)return await StopEditor(f.TripId,400,ct,f);
        try {
            var result=await stopAppend.Append(Actor,f.TripId,f.Revision,new(f.Name,f.Address,f.OffsetMinutes,f.CanPickup,f.CanDropoff),ct);
            if(result.Plan.Blocked){ModelState.AddModelError("",result.Plan.Message);return await StopEditor(f.TripId,409,ct,f);}
            TempData["Notice"]=result.Applied?"Đã thêm điểm mới. Các điểm đã có được giữ nguyên.":result.Plan.Message;
            return RedirectToAction(nameof(TripStops),new{id=f.TripId});
        }catch(BusinessException e){ModelState.AddModelError("",e.Message);return await StopEditor(f.TripId,e.Status,ct,f);}
    }
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> SpecificStop(SpecificStopForm f,CancellationToken ct) {
        Guard.Input(f.TripId>0,"Chuyến không hợp lệ.");
        if(!ModelState.IsValid||!f.OperatorConfirmed) {
            if(!f.OperatorConfirmed)ModelState.AddModelError("","Cần xác nhận địa điểm, chiều tuyến, giờ và quyền đón/trả của chuyến.");
            ViewBag.SpecificForm=f;return await StopEditor(f.TripId,400,ct);
        }
        try {
            var result=await stopAppend.Specific(Actor,f.TripId,f.Revision,[new(f.Name,f.Address,f.OffsetMinutes,f.CanPickup,f.CanDropoff,f.Area,f.SourceReference,f.OperatorConfirmed)],false,ct);
            if(result.Plan.Blocked){ModelState.AddModelError("",result.Plan.Message);ViewBag.SpecificForm=f;return await StopEditor(f.TripId,409,ct);}
            TempData["Notice"]=result.Applied?"Đã bổ sung điểm cụ thể cho lượt đặt mới. Thông tin đã giữ/đặt vẫn được bảo toàn.":result.Plan.Message;
            return RedirectToAction(nameof(TripStops),new{id=f.TripId});
        }catch(BusinessException e){ModelState.AddModelError("",e.Message);ViewBag.SpecificForm=f;return await StopEditor(f.TripId,e.Status,ct);}
    }
    public async Task<IActionResult> Users() {
        var list=await users.Users.OrderBy(x=>x.Email).Take(200).ToListAsync();var map=new Dictionary<string,string>();
        foreach(var u in list)map[u.Id]=string.Join(", ",await users.GetRolesAsync(u));ViewBag.RoleMap=map;return View(list);
    }
    [HttpPost] public async Task<IActionResult> UserRole(RoleForm f) {if(!ModelState.IsValid)throw new BusinessException(400,"Người dùng/vai trò không hợp lệ.");await catalog.SetRole(Actor,f);return RedirectToAction(nameof(Users));}
    public async Task<IActionResult> Audit()=>View(await db.AuditEntries.AsNoTracking().OrderByDescending(x=>x.Id).Take(200).ToListAsync());
}
