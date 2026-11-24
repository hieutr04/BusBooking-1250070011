namespace BusBooking.Services;
/// <summary>GET-only, role-scoped local return destinations. Authorization remains in controllers.</summary>
public static class LoginDestination {
    public static string Select(IEnumerable<string> roles,string? returnUrl) {
        var set=roles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var role=new[]{"Admin","Staff","Driver","Customer"}.FirstOrDefault(set.Contains)??"Customer";
        var fallback=role switch {"Admin"=>"/Admin","Staff"=>"/Staff","Driver"=>"/Driver",_=>"/"};
        if(string.IsNullOrWhiteSpace(returnUrl)||returnUrl.Any(c=>char.IsControl(c))||returnUrl.Contains('\\')||returnUrl.Contains('#'))return fallback;
        var url=returnUrl.StartsWith("~/",StringComparison.Ordinal)?returnUrl[1..]:returnUrl;
        if(!url.StartsWith('/')||url.StartsWith("//",StringComparison.Ordinal))return fallback;
        var path=url.Split('?',2)[0];
        // Encoded paths, dot segments and repeated slashes are not accepted as redirect routes.
        if(path.Contains('%')||path.Contains("//",StringComparison.Ordinal)||path.Split('/').Any(s=>s is "." or ".."))return fallback;
        var parts=path.Trim('/').Split('/',StringSplitOptions.RemoveEmptyEntries);
        var controller=parts.Length>0?parts[0].ToLowerInvariant():"home";
        var action=parts.Length>1?parts[1].ToLowerInvariant():"index";
        if(parts.Length>3)return fallback;
        var id=parts.Length==3?parts[2]:null;
        var publicGet=controller=="home"&&(action is "index" or "policy")&&id==null;
        var tripGet=controller=="trips"&&(action=="index"&&id==null||action=="details"&&PositiveId(id));
        var bookingGet=controller=="bookings"&&(action=="index"&&id==null||action=="details"&&GuidId(id));
        var profile=controller=="account"&&(action is "profile" or "changepassword")&&id==null;
        var adminGet=controller=="admin"&&(
            new[]{"index","routes","drivers","vehicles","layouts","trips","users","audit"}.Contains(action)&&id==null
            ||new[]{"routeedit","vehicleedit","layoutedit","tripedit"}.Contains(action)&&PositiveId(id)
            ||action=="driveredit"&&GuidId(id)
            ||action=="driverpassword"&&id!=null&&GuidId(id));
        var staffGet=controller=="staff"&&(new[]{"index","requests","trips","sell"}.Contains(action)&&id==null||action=="passengers"&&PositiveId(id));
        var driverGet=controller=="driver"&&(action=="index"&&id==null||action=="details"&&PositiveId(id));
        var allowed=role switch {
            "Admin"=>adminGet||staffGet||(bookingGet&&action=="details")||profile,
            "Staff"=>staffGet||bookingGet||tripGet||profile,
            "Driver"=>driverGet||profile,
            _=>publicGet||tripGet||bookingGet||profile
        };
        return allowed?url:fallback;
    }
    private static bool PositiveId(string? id)=>id==null||int.TryParse(id,out var n)&&n>0;
    private static bool GuidId(string? id)=>id==null||Guid.TryParse(id,out var guid)&&guid!=Guid.Empty;
}
