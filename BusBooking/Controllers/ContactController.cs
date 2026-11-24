using BusBooking.Services;
using BusBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Controllers;

[AllowAnonymous]
[ResponseCache(NoStore=true, Location=ResponseCacheLocation.None)]
public sealed class ContactController(ContactService service, ContactTokens tokens, ILogger<ContactController> log) : Controller {
    [HttpGet("/lien-he")]
    public IActionResult Index() => View(new ContactForm { SubmissionToken=tokens.Create() });

    [HttpPost("/lien-he")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("contact-submit")]
    [RequestSizeLimit(65536)]
    [RequestFormLimits(ValueCountLimit=12, ValueLengthLimit=12000)]
    public async Task<IActionResult> Index(ContactForm form, CancellationToken ct) {
        if (!tokens.TryRead(form.SubmissionToken, out var key)) {
            ModelState.Remove(nameof(form.SubmissionToken));
            form.SubmissionToken=tokens.Create();
            ModelState.AddModelError("", "Biểu mẫu đã hết hạn hoặc không hợp lệ. Nội dung được giữ lại; vui lòng bấm Gửi liên hệ lần nữa.");
        }
        if (!ModelState.IsValid) { Response.StatusCode=400; return View(form); }
        try { await service.Record(form, key, ct); }
        catch (BusinessException ex) {
            ModelState.AddModelError("", ex.Message);
            ModelState.Remove(nameof(form.SubmissionToken)); form.SubmissionToken=tokens.Create();
            Response.StatusCode=ex.Status; return View(form);
        }
        catch (Exception ex) when (ex is DbUpdateException or SqlException) {
            var sql=ex as SqlException ?? ex.InnerException as SqlException;
            // Deliberately omit exception/SQL parameters, contact text and personal details.
            log.LogWarning("Contact persistence failed. SQL number {SqlNumber}; request {RequestId}", sql?.Number, HttpContext.TraceIdentifier);
            ModelState.AddModelError("", "Hệ thống chưa ghi nhận được liên hệ. Vui lòng thử lại sau; nội dung của bạn vẫn được giữ trong biểu mẫu.");
            if (User.IsInRole("Admin"))
                ModelState.AddModelError("", "Quản trị viên: vui lòng kiểm tra quyền ghi dữ liệu và bản cập nhật trước khi tiếp nhận liên hệ.");
            Response.StatusCode=503; return View(form);
        }
        TempData["Notice"]="Đã ghi nhận liên hệ trong hệ thống.";
        return RedirectToAction(nameof(Index));
    }
}
