using System.Globalization;
using System.Text;
namespace BusBooking.Services;
public sealed record PhotoAsset(string Key, string Path, string Alt, string Author, string SourceUrl, string License, string LicenseUrl, int Width, int Height) {
    public string OriginalTitle => Key switch { "coach-hero"=>"Mercedes-Benz Tourismo Länsiväylä 20260906", "coach-exterior"=>"Mercedes-Benz O 350 Tourismo 01", "da-lat"=>"Xuan Huong Lake", "nha-trang"=>"Nha Trang - Hòn Đỏ island panoramic Mar 2024", "ho-chi-minh-city"=>"Ho Chi Minh City skyline", "contact-journey"=>"Hai Van Pass, Vietnam, View northwards", _=>Alt };
    public string Position => Key switch { "ho-chi-minh-city"=>"20% center", "da-lat"=>"50% 60%", _=>"50% center" };
}
public sealed record PhotoDisplay(PhotoAsset Asset, bool Lazy);
public static class PhotoCatalog {
    public static readonly PhotoAsset CoachHero = new("coach-hero", "~/images/buses/coach-hero.webp", "Xe khách đường dài màu trắng trên đường tại Espoo, Phần Lan", "VynedJ", "https://commons.wikimedia.org/wiki/File:Mercedes-Benz_Tourismo_L%C3%A4nsiv%C3%A4yl%C3%A4_20260906.jpg", "CC BY 4.0", "https://creativecommons.org/licenses/by/4.0", 1600, 1143);
    public static readonly PhotoAsset CoachExterior = new("coach-exterior", "~/images/buses/coach-exterior.webp", "Xe khách đường dài Mercedes-Benz Tourismo tại Uetersen, Đức", "Frank Schwichtenberg", "https://commons.wikimedia.org/wiki/File:Mercedes-Benz_O_350_Tourismo_01.jpg", "CC BY 3.0", "https://creativecommons.org/licenses/by/3.0", 640, 427);
    public static readonly PhotoAsset DaLat = new("da-lat", "~/images/destinations/da-lat.webp", "Hồ Xuân Hương, Đà Lạt", "Tu", "https://commons.wikimedia.org/wiki/File:Xuan_Huong_Lake.jpg", "CC BY 2.0", "https://creativecommons.org/licenses/by/2.0", 1280, 756);
    public static readonly PhotoAsset NhaTrang = new("nha-trang", "~/images/destinations/nha-trang.webp", "Hòn Đỏ và chùa Từ Tôn, Nha Trang", "Dominic Nelson", "https://commons.wikimedia.org/wiki/File:Nha_Trang_-_H%C3%B2n_%C4%90%E1%BB%8F_island_panoramic_Mar_2024.jpg", "CC BY-SA 4.0", "https://creativecommons.org/licenses/by-sa/4.0", 1600, 557);
    public static readonly PhotoAsset HoChiMinhCity = new("ho-chi-minh-city", "~/images/destinations/ho-chi-minh-city.webp", "Đường chân trời TP. Hồ Chí Minh về đêm, ảnh năm 2011", "Tri Nguyen", "https://commons.wikimedia.org/wiki/File:Ho_Chi_Minh_City_skyline.jpg", "CC BY 2.0", "https://creativecommons.org/licenses/by/2.0", 1500, 454);
    public static readonly PhotoAsset Journey = new("contact-journey", "~/images/banners/contact-journey.webp", "Đường đèo Hải Vân nhìn về phía bắc", "Vyacheslav Argenberg", "https://commons.wikimedia.org/wiki/File:Hai_Van_Pass,_Vietnam,_View_northwards.jpg", "CC BY 4.0", "https://creativecommons.org/licenses/by/4.0", 1600, 1067);
    public static readonly PhotoAsset Neutral = new("neutral", "~/images/landscape.svg", "Hình minh họa hành trình", "Bus Center", "", "SVG minh họa nội bộ", "", 600, 240);
    public static IReadOnlyList<PhotoAsset> All { get; } = Array.AsReadOnly(new[] { CoachHero, CoachExterior, DaLat, NhaTrang, HoChiMinhCity, Journey });
    public static PhotoAsset Destination(string? destination) {
        var key=Normalize(destination);
        return key switch {
            "da lat" or "tp da lat" or "thanh pho da lat" => DaLat,
            "nha trang" or "tp nha trang" or "thanh pho nha trang" => NhaTrang,
            "ho chi minh" or "tp ho chi minh" or "thanh pho ho chi minh" or "tp hcm" or "tphcm" or "hcm" or "sai gon" => HoChiMinhCity,
            _ => Neutral
        };
    }
    private static string Normalize(string? value) {
        if(string.IsNullOrWhiteSpace(value))return "";
        var text=value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var output=new StringBuilder();
        foreach(var c in text) {
            if(CharUnicodeInfo.GetUnicodeCategory(c)==UnicodeCategory.NonSpacingMark)continue;
            if(char.IsLetterOrDigit(c))output.Append(c=='đ'?'d':c);
            else output.Append(' ');
        }
        return string.Join(" ",output.ToString().Split(' ',StringSplitOptions.RemoveEmptyEntries));
    }
}
