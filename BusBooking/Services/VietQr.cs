using System.Globalization;
using System.Text;
using QRCoder;

namespace BusBooking.Services;

public static class VietQr {
    public static string? CreateImage(string? bankBin,string account,decimal amount,string bookingCode) {
        if(bankBin is not {Length:6}||!bankBin.All(char.IsAsciiDigit)||
            account.Length is <1 or >19||!account.All(char.IsAsciiDigit)||
            amount<=0||decimal.Truncate(amount)!=amount||amount>9999999999999m||
            bookingCode.Length is <1 or >22||!bookingCode.All(char.IsAsciiLetterOrDigit))return null;

        var beneficiary=Field("00",bankBin)+Field("01",account);
        var transfer=Field("00","A000000727")+Field("01",beneficiary)+Field("02","QRIBFTTA");
        var payload=Field("00","01")+Field("01","12")+Field("38",transfer)+Field("53","704")+
            Field("54",amount.ToString("0",CultureInfo.InvariantCulture))+Field("58","VN")+
            Field("62",Field("08",Memo(bookingCode)))+"6304";
        payload+=Checksum(payload);
        var png=PngByteQRCodeHelper.GetQRCode(payload,QRCodeGenerator.ECCLevel.M,6);
        return "data:image/png;base64,"+Convert.ToBase64String(png);
    }

    public static string Memo(string bookingCode)=>"BC "+bookingCode;

    private static string Field(string id,string value)=>
        id+value.Length.ToString("00",CultureInfo.InvariantCulture)+value;

    private static string Checksum(string payload) {
        ushort crc=0xffff;
        foreach(var value in Encoding.ASCII.GetBytes(payload)){
            crc^=(ushort)(value<<8);
            for(var bit=0;bit<8;bit++){
                crc=(ushort)((crc<<1)^((crc&0x8000)!=0?0x1021:0));
            }
        }
        return crc.ToString("X4",CultureInfo.InvariantCulture);
    }
}
