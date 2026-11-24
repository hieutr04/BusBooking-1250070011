using System.Globalization;
using System.Text;
namespace BusBooking.Services;

// Presentation only. Never rewrite TripStop entities or historical Booking snapshots.
public static class StopPresentation {
    private static readonly string[] Prefixes=["Điểm học tập · ","Điểm mô phỏng · ","Điểm đầu · ","Điểm cuối · "];
    public static string Name(string? value) {
        var text=value?.Trim()??"";
        foreach(var prefix in Prefixes)
            if(text.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))return text[prefix.Length..].Trim();
        return text;
    }
    public static string? Address(string? value) {
        var text=value?.Trim();
        return string.IsNullOrWhiteSpace(text)||text is "Chưa cấu hình" or "Chưa cập nhật" or "Điểm trong dữ liệu học tập; chưa cấu hình địa chỉ cụ thể." ? null : text;
    }
    public static bool InternalDescription(string? value)=>value?.Trim()=="Dữ liệu học tập; không phải lịch/giá bán của doanh nghiệp.";
    public static string? Description(string? value)=>InternalDescription(value)?null:value;
    public static string Duration(int minutes) {
        var hours=Math.Max(0,minutes)/60;var rest=Math.Max(0,minutes)%60;
        return hours==0?$"{rest} phút":rest==0?$"{hours} giờ":$"{hours} giờ {rest} phút";
    }
    public static string Label(string? value) {
        var text=value??"";
        foreach(var suffix in new[]{" — dữ liệu thử"," — tài khoản thử nghiệm"})
            if(text.EndsWith(suffix,StringComparison.Ordinal))return text[..^suffix.Length];
        return text;
    }
    public static string Key(string? value) {
        var text=Name(value).ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var b=new StringBuilder();
        foreach(var c in text) {
            if(CharUnicodeInfo.GetUnicodeCategory(c)==UnicodeCategory.NonSpacingMark)continue;
            b.Append(char.IsLetterOrDigit(c)?c=='đ'?'d':c:' ');
        }
        var key=string.Join(" ",b.ToString().Split(' ',StringSplitOptions.RemoveEmptyEntries));
        if(key.StartsWith("thanh pho "))key=key[10..];
        else if(key.StartsWith("tp "))key=key[3..];
        return key is "hcm" or "tphcm" or "sai gon"?"ho chi minh":key;
    }
}
