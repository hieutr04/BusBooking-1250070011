using System.ComponentModel.DataAnnotations;
namespace BusBooking.ViewModels;

public sealed class ContactForm : IValidatableObject {
    [Required(ErrorMessage="Vui lòng nhập họ và tên.")]
    [StringLength(100, MinimumLength=2, ErrorMessage="Họ tên cần từ 2 đến 100 ký tự.")]
    [Display(Name="Họ và tên")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage="Vui lòng nhập email liên hệ.")]
    [EmailAddress(ErrorMessage="Email liên hệ không hợp lệ.")]
    [StringLength(254, ErrorMessage="Email tối đa 254 ký tự.")]
    [Display(Name="Email liên hệ")]
    public string Email { get; set; } = "";

    [StringLength(25, ErrorMessage="Số điện thoại tối đa 25 ký tự.")]
    [RegularExpression(@"^\+?[0-9 ()\-.]{7,25}$", ErrorMessage="Số điện thoại cần 7–25 ký tự, gồm số và dấu +, cách, (), - hoặc .")]
    [Display(Name="Số điện thoại (tùy chọn)")]
    public string? Phone { get; set; }

    [Required(ErrorMessage="Vui lòng nhập tiêu đề.")]
    [StringLength(160, MinimumLength=3, ErrorMessage="Tiêu đề cần từ 3 đến 160 ký tự.")]
    [Display(Name="Tiêu đề")]
    public string Subject { get; set; } = "";

    [Required(ErrorMessage="Vui lòng nhập nội dung liên hệ.")]
    [StringLength(4000, MinimumLength=10, ErrorMessage="Nội dung cần từ 10 đến 4.000 ký tự.")]
    [Display(Name="Nội dung liên hệ")]
    public string Body { get; set; } = "";

    [Required(ErrorMessage="Biểu mẫu hết hạn. Vui lòng gửi lại.")]
    [StringLength(1024, ErrorMessage="Mã biểu mẫu không hợp lệ.")]
    public string SubmissionToken { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(ValidationContext context) {
        if (!string.IsNullOrWhiteSpace(FullName) && FullName.Trim().Length<2)
            yield return new ValidationResult("Họ tên cần ít nhất 2 ký tự sau khi bỏ khoảng trắng đầu/cuối.",new[]{nameof(FullName)});
        if (!string.IsNullOrWhiteSpace(Subject) && Subject.Trim().Length<3)
            yield return new ValidationResult("Tiêu đề cần ít nhất 3 ký tự sau khi bỏ khoảng trắng đầu/cuối.",new[]{nameof(Subject)});
        if (!string.IsNullOrWhiteSpace(Body) && Body.Trim().Length<10)
            yield return new ValidationResult("Nội dung cần ít nhất 10 ký tự sau khi bỏ khoảng trắng đầu/cuối.",new[]{nameof(Body)});
        if (!string.IsNullOrWhiteSpace(Phone) && (Phone.Count(char.IsDigit)<7 || Phone.Count(char.IsDigit)>15))
            yield return new ValidationResult("Số điện thoại cần từ 7 đến 15 chữ số.",new[]{nameof(Phone)});
    }
}

public sealed record ContactSummary(Guid Id, string FullName, string Email, string Subject, DateTime CreatedUtc, bool IsRead);
public sealed record ContactInbox(IReadOnlyList<ContactSummary> Items, int Page, int Total, bool UnreadOnly) {
    public int PageCount => Math.Max(1, (Total + 19) / 20);
}
