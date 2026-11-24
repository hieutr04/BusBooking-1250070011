using System.Text;
using System.Globalization;

namespace BusBooking.Services;

public static class CsvDownload {
    public static byte[] Encode(IEnumerable<object[]> rows) {
        var content=new StringBuilder();
        foreach(var row in rows)
            content.AppendJoin(',',row.Select(Cell)).Append("\r\n");
        var utf8=new UTF8Encoding(encoderShouldEmitUTF8Identifier:true);
        return [..utf8.GetPreamble(),..utf8.GetBytes(content.ToString())];
    }

    private static string Cell(object field) {
        if(field is decimal number)return number.ToString(CultureInfo.InvariantCulture);
        var value=(string)field;
        var first=value.TrimStart().FirstOrDefault();
        if(first is '=' or '+' or '-' or '@' or '＝' or '＋' or '－' or '＠'||value.StartsWith('\t')||value.StartsWith('\r')||value.StartsWith('\n'))
            value="'"+value;
        return "\""+value.Replace("\"","\"\"")+"\"";
    }
}
