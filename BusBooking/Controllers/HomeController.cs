using BusBooking.Data;
using BusBooking.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Controllers;
public class HomeController(AppDbContext db):Controller {
    public async Task<IActionResult> Index()=>View(await db.Routes.AsNoTracking().Where(x=>x.IsActive).OrderBy(x=>x.Origin).ToListAsync());
    public IActionResult Policy()=>View();
    [Route("Home/Status/{code:int}")] public IActionResult StatusPage(int code) {Response.StatusCode=code;return View("~/Views/Shared/Problem.cshtml",new ProblemVm(code,code==403?"Bạn không có quyền truy cập khu vực này.":"Không tìm thấy nội dung hoặc yêu cầu không hợp lệ.",HttpContext.TraceIdentifier));}
    [IgnoreAntiforgeryToken] public IActionResult Error(){Response.StatusCode=500;return View("~/Views/Shared/Problem.cshtml",new ProblemVm(500,"Chưa thể hoàn tất yêu cầu. Vui lòng thử lại hoặc liên hệ hỗ trợ kèm mã yêu cầu.",HttpContext.TraceIdentifier));}
    [HttpGet("health")] public async Task<IActionResult> Health() {try{return await db.Database.CanConnectAsync()?Ok(new{database="SqlServer",connected=true}):StatusCode(503,new{connected=false});}catch{return StatusCode(503,new{connected=false});}}
}
