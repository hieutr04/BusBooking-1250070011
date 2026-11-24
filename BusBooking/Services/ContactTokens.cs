using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
namespace BusBooking.Services;

public sealed class ContactTokens {
    private readonly ITimeLimitedDataProtector protector;
    public ContactTokens(IDataProtectionProvider provider) =>
        protector = provider.CreateProtector("BusBooking.Contact.Submission.v1").ToTimeLimitedDataProtector();
    public string Create() => protector.Protect(Guid.NewGuid().ToString("D"), TimeSpan.FromHours(2));
    public bool TryRead(string? token, out Guid key) {
        key = Guid.Empty;
        if (string.IsNullOrWhiteSpace(token) || token.Length > 1024) return false;
        try { return Guid.TryParseExact(protector.Unprotect(token), "D", out key) && key != Guid.Empty; }
        catch (CryptographicException) { return false; }
    }
}
