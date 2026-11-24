namespace BusBooking.Models;

public sealed class ContactMessage {
    public Guid Id { get; set; }
    public Guid SubmissionKey { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
    public bool IsRead { get; set; }
}
