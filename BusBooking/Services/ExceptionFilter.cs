using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace BusBooking.Services;
public record ProblemVm(int Status,string Message,string RequestId);
public class ExceptionFilter(ILogger<ExceptionFilter> log,IModelMetadataProvider provider):IExceptionFilter {
    public void OnException(ExceptionContext c) {
        var sql=c.Exception as SqlException??c.Exception.InnerException as SqlException;
        int status;string message;
        if(c.Exception is BusinessException b){status=b.Status;message=b.Message;}
        else if(sql?.Number is 1205 or 1222 or 2601 or 2627||c.Exception is DbUpdateConcurrencyException){status=409;message="Thông tin vừa thay đổi. Vui lòng tải lại và kiểm tra trạng thái trước khi thử lại.";}
        else if(sql!=null){status=503;message="Dịch vụ tạm thời chưa sẵn sàng. Vui lòng thử lại sau.";}
        else {status=500;message="Chưa thể hoàn tất yêu cầu. Vui lòng thử lại hoặc cung cấp mã yêu cầu cho bộ phận hỗ trợ.";}
        if(status>=500)log.LogError(c.Exception,"Request {RequestId} failed",c.HttpContext.TraceIdentifier);
        var p=new ProblemVm(status,message,c.HttpContext.TraceIdentifier);
        if(c.HttpContext.Request.Path.StartsWithSegments("/api")) c.Result=new ObjectResult(p){StatusCode=status};
        else c.Result=new ViewResult{ViewName="~/Views/Shared/Problem.cshtml",StatusCode=status,ViewData=new ViewDataDictionary<ProblemVm>(provider,c.ModelState){Model=p}};
        c.ExceptionHandled=true;
    }
}
