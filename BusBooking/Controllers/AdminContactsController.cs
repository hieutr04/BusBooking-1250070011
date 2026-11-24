using BusBooking.Data;
using BusBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Controllers;

[Authorize(Roles="Admin")]
[ResponseCache(NoStore=true, Location=ResponseCacheLocation.None)]
public sealed class AdminContactsController(AppDbContext db) : Controller {
    [HttpGet("/Admin/Contacts")]
    public async Task<IActionResult> Index(int page=1, bool unread=false, CancellationToken ct=default) {
        page=Math.Clamp(page,1,100000);
        var query=db.ContactMessages.AsNoTracking();
        if (unread) query=query.Where(x=>!x.IsRead);
        var total=await query.CountAsync(ct);
        page=Math.Min(page,Math.Max(1,(total+19)/20));
        var items=await query.OrderByDescending(x=>x.CreatedUtc).ThenByDescending(x=>x.Id)
            .Skip((page-1)*20).Take(20)
            .Select(x=>new ContactSummary(x.Id,x.FullName,x.Email,x.Subject,x.CreatedUtc,x.IsRead)).ToListAsync(ct);
        return View(new ContactInbox(items,page,total,unread));
    }
    [HttpGet("/Admin/Contacts/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct) {
        var message=await db.ContactMessages.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
        return message==null ? NotFound() : View(message);
    }
    [HttpPost("/Admin/Contacts/{id:guid}/read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct) {
        var count=await db.ContactMessages.Where(x=>x.Id==id).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.IsRead,true),ct);
        if (count==0) return NotFound();
        TempData["Notice"]="Đã đánh dấu liên hệ là đã đọc.";
        return RedirectToAction(nameof(Details),new {id});
    }
}
