using System.Net.Mail;
using System.Text.RegularExpressions;
namespace BusBooking.Services;

public sealed class ContactOptions {
    public string LegalName { get; set; } = "";
    public string SupportWebsite { get; set; } = "";
    public string SupportHours { get; set; } = "";
    public string? ValidWebsite => Uri.TryCreate(SupportWebsite?.Trim(),UriKind.Absolute,out var uri) && (uri.Scheme==Uri.UriSchemeHttps||uri.Scheme==Uri.UriSchemeHttp) && string.IsNullOrEmpty(uri.UserInfo) ? uri.AbsoluteUri : null;
    public string SupportEmail { get; set; } = "";
    public string SupportHotline { get; set; } = "";
    public string FacebookUrl { get; set; } = "";
    public string ZaloUrl { get; set; } = "";
    public string SupportPhone { get; set; } = "";
    public string SupportAddress { get; set; } = "";
    public string? ValidEmail => MailAddress.TryCreate(SupportEmail?.Trim(), out var address)
        && address.Address == SupportEmail?.Trim() ? address.Address : null;
    public string? ValidPhone => ValidNumber(SupportPhone);
    public string? ValidHotline => ValidNumber(SupportHotline);
    public string? PhoneLink => Dial(ValidPhone);
    public string? HotlineLink => Dial(ValidHotline);
    public string? FacebookLink => SocialLink(FacebookUrl,"facebook.com");
    public string? ZaloLink => SocialLink(ZaloUrl,"zalo.me");

    private static string? ValidNumber(string? value) => value?.Trim() is {Length:>0} number
        && Regex.IsMatch(number,@"^\+?[0-9 ()\-.]{7,25}$")
        && Regex.IsMatch(Dial(number)!,@"^\+?[0-9]{7,15}$") ? number : null;
    private static string? Dial(string? number)=>number is null?null:Regex.Replace(number,@"[ ()\-.]","");
    private static string? SocialLink(string? value,string host)=>
        Uri.TryCreate(value?.Trim(),UriKind.Absolute,out var uri)&&uri.Scheme==Uri.UriSchemeHttps
        &&(uri.Host==host||uri.Host=="www."+host)&&uri.UserInfo.Length==0&&uri.AbsolutePath!="/"
        ?uri.AbsoluteUri:null;
}
