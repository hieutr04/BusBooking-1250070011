using BusBooking.Data;
using BusBooking.Models;
using BusBooking.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Services;

public sealed class ContactService(AppDbContext db) {
    public async Task Record(ContactForm form, Guid submissionKey, CancellationToken ct) {
        var message = new ContactMessage {
            Id=Guid.NewGuid(), SubmissionKey=submissionKey,
            FullName=form.FullName.Trim(), Email=form.Email.Trim(),
            Phone=string.IsNullOrWhiteSpace(form.Phone) ? null : form.Phone.Trim(),
            Subject=form.Subject.Trim(), Body=form.Body.Trim(), CreatedUtc=DateTime.UtcNow, IsRead=false
        };
        var previous = await db.ContactMessages.AsNoTracking().SingleOrDefaultAsync(x=>x.SubmissionKey==submissionKey, ct);
        if (previous != null) { RequireSame(previous, message); return; }
        db.ContactMessages.Add(message);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 }) {
            db.Entry(message).State=EntityState.Detached;
            previous=await db.ContactMessages.AsNoTracking().SingleOrDefaultAsync(x=>x.SubmissionKey==submissionKey, ct);
            if (previous==null) throw;
            RequireSame(previous, message);
        }
    }
    private static void RequireSame(ContactMessage a, ContactMessage b) {
        if (a.FullName!=b.FullName || a.Email!=b.Email || a.Phone!=b.Phone || a.Subject!=b.Subject || a.Body!=b.Body)
            throw new BusinessException(409,"Biểu mẫu này đã được gửi với nội dung khác. Kiểm tra nội dung và bấm Gửi liên hệ một lần nữa để tạo liên hệ mới.");
    }
}
