using BusBooking.BackgroundServices;
using Microsoft.AspNetCore.Authentication;
using BusBooking.Data;
using BusBooking.Hubs;
using BusBooking.Models;
using BusBooking.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading.RateLimiting;
var builder=WebApplication.CreateBuilder(args);
var connection=builder.Configuration.GetConnectionString("DefaultConnection");
if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("Chưa cấu hình SQL Server. Tại thư mục solution chạy: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<connection string từ SQL_SERVER_SETUP.md>\" --project BusBooking. Không có database fallback.");
builder.Services.AddDbContext<AppDbContext>(o=>o.UseSqlServer(connection,sql=>sql.CommandTimeout(30)));
builder.Services.AddIdentity<AppUser,IdentityRole>(o=> {
    o.User.RequireUniqueEmail=true;o.Lockout.MaxFailedAccessAttempts=5;
    o.Password.RequiredLength=AppUser.MinimumPasswordLength;
    o.Password.RequireDigit=false;
    o.Password.RequireLowercase=false;
    o.Password.RequireUppercase=false;
    o.Password.RequireNonAlphanumeric=false;
    o.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(10);
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o=> {
    o.LoginPath="/Account/Login";o.AccessDeniedPath="/Home/Status/403";o.Cookie.Name="BusBooking.Auth";
    o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Lax;o.ExpireTimeSpan=TimeSpan.FromHours(4);
    o.Events.OnRedirectToLogin=c=>{if(c.Request.Path.StartsWithSegments("/api")){c.Response.StatusCode=401;return Task.CompletedTask;}c.Response.Redirect(c.RedirectUri);return Task.CompletedTask;};
    o.Events.OnRedirectToAccessDenied=c=>{if(c.Request.Path.StartsWithSegments("/api")){c.Response.StatusCode=403;return Task.CompletedTask;}c.Response.Redirect(c.RedirectUri);return Task.CompletedTask;};
    o.Events.OnValidatePrincipal=async c=> {
        await SecurityStampValidator.ValidatePrincipalAsync(c);
        if(c.Principal?.Identity?.IsAuthenticated==true) {
            var user=await c.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>().GetUserAsync(c.Principal);
            if(user==null||!user.IsActive) {c.RejectPrincipal();await c.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);}
        }
    };
});
builder.Services.Configure<SecurityStampValidatorOptions>(o=>o.ValidationInterval=TimeSpan.Zero);
builder.Services.AddOptions<BookingOptions>().BindConfiguration("Booking").Validate(x=>x.HoldSeconds==300,"Phạm vi đề tài yêu cầu HoldSeconds = 300.")
    .Validate(x=>x.ExpiryScanSeconds>=1&&x.ExpiryScanSeconds<=60&&x.ExpiryBatchSize>=1&&x.ExpiryBatchSize<=500&&x.MaxSeats>=1&&x.MaxSeats<=6&&x.ScheduleBufferMinutes>=0&&x.ArriveEarlyMinutes>=0&&x.ArriveEarlyMinutes<=120,"Booking options không hợp lệ.").ValidateOnStart();
builder.Services.AddControllersWithViews(o=> {
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());o.Filters.Add<ExceptionFilter>();
    o.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_=>"Vui lòng nhập giá trị hợp lệ.");
});
builder.Services.AddAntiforgery(o=>o.HeaderName="X-CSRF-TOKEN");
builder.Services.AddDataProtection();
builder.Services.Configure<ContactOptions>(builder.Configuration.GetSection("Contact"));
builder.Services.AddSingleton<ContactTokens>();
builder.Services.AddScoped<ContactService>();
builder.Services.AddScoped<TicketCheckIn>();
builder.Services.AddScoped<TripStopService>();
builder.Services.AddScoped<TripStopAppendService>();
builder.Services.AddRateLimiter(options=> {
    options.RejectionStatusCode=StatusCodes.Status429TooManyRequests;
    options.AddPolicy("contact-submit", context=>RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString()??"unknown",
        _=>new FixedWindowRateLimiterOptions { PermitLimit=8, Window=TimeSpan.FromMinutes(10), QueueLimit=0, AutoReplenishment=true }));
    options.OnRejected=async (context,ct)=> {
        context.HttpContext.Response.ContentType="text/html; charset=utf-8";
        if(context.Lease.TryGetMetadata(MetadataName.RetryAfter,out var retry))
            context.HttpContext.Response.Headers["Retry-After"]=Math.Ceiling(retry.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        await context.HttpContext.Response.WriteAsync("<!doctype html><html lang='vi'><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'><title>Vui lòng chờ</title><h1>Bạn gửi liên hệ quá nhanh</h1><p>Vui lòng chờ tối đa 10 phút rồi thử lại. Có thể dùng nút Quay lại của trình duyệt để xem nội dung vừa nhập.</p><a href='/lien-he'>Trang Liên hệ</a></html>",ct);
    };
});
builder.Services.AddSignalR(o=>{o.MaximumReceiveMessageSize=8*1024;});
builder.Services.AddSingleton<RealtimeMetrics>();builder.Services.AddScoped<DbGate>();builder.Services.AddScoped<BookingService>();builder.Services.AddScoped<CatalogService>();builder.Services.AddScoped<ReportService>();builder.Services.AddScoped<RealtimePublisher>();
builder.Services.AddHostedService<HoldExpiryWorker>();
var app=builder.Build();
CultureInfo.DefaultThreadCurrentCulture=CultureInfo.GetCultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentUICulture=CultureInfo.GetCultureInfo("vi-VN");
if(args.Contains("--review-stop-plan")||args.Contains("--apply-stop-plan")||args.Contains("--seed")||args.Contains("--seed-highlands")||args.Contains("--seed-paired-fleet")||args.Contains("--complete-user-profiles")||args.Contains("--check-db")||args.Contains("--update-trip-stops")||args.Contains("--review-dalat-stops")||args.Contains("--complete-dalat-stops")) {
    using var scope=app.Services.CreateScope();var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if(args.Contains("--review-stop-plan")||args.Contains("--apply-stop-plan")) {
        Guard.Input(!(args.Contains("--review-stop-plan")&&args.Contains("--apply-stop-plan")),"Chỉ chọn xem trước hoặc áp dụng.");
        var flag=args.Contains("--review-stop-plan")?"--review-stop-plan":"--apply-stop-plan";
        var index=Array.IndexOf(args,flag);Guard.Input(index+1<args.Length&&!args[index+1].StartsWith("--"),"Thiếu đường dẫn tệp kế hoạch.");
        var reportIndex=Array.IndexOf(args,"--stop-report");
        Guard.Input(reportIndex<0||(reportIndex+1<args.Length&&!args[reportIndex+1].StartsWith("--")),"Thiếu đường dẫn báo cáo.");
        var report=reportIndex>=0?args[reportIndex+1]:Path.Combine(app.Environment.ContentRootPath,flag=="--review-stop-plan"?"specific-stop-preview.json":"specific-stop-report.json");
        if(!await SpecificStopMaintenance.Run(db,scope.ServiceProvider.GetRequiredService<TripStopAppendService>(),args[index+1],flag=="--review-stop-plan",report))Environment.ExitCode=2;
    }
    else if(args.Contains("--review-dalat-stops")||args.Contains("--complete-dalat-stops")) {
        if(args.Contains("--review-dalat-stops")&&args.Contains("--complete-dalat-stops"))throw new ArgumentException("Chỉ chọn xem trước hoặc áp dụng.");
        var index=Array.IndexOf(args,"--stop-report");
        var path=index>=0?(index+1<args.Length?args[index+1]:throw new ArgumentException("Thiếu đường dẫn --stop-report.")):
            Path.Combine(app.Environment.ContentRootPath,args.Contains("--review-dalat-stops")?"dalat-stops-preview.csv":"dalat-stops-report.csv");
        var ok=await DalatStopMaintenance.Run(db,scope.ServiceProvider.GetRequiredService<TripStopAppendService>(),args.Contains("--review-dalat-stops"),path);
        if(!ok)Environment.ExitCode=2;
    }
    else if(args.Contains("--update-trip-stops")) {
        await TripStopMaintenance.Run(db,scope.ServiceProvider.GetRequiredService<DbGate>(),
            args.Contains("--study-stops"),Path.Combine(app.Environment.ContentRootPath,"trip-stops-report.csv"));
        if(args.Contains("--study-stops")) {
            db.ChangeTracker.Clear();
            if(!await DalatStopMaintenance.Run(db,scope.ServiceProvider.GetRequiredService<TripStopAppendService>(),false,Path.Combine(app.Environment.ContentRootPath,"dalat-stops-report.csv")))Environment.ExitCode=2;
        }
    }
    else if(args.Contains("--seed"))await SeedData.Run(scope.ServiceProvider);
    else if(args.Contains("--seed-highlands"))await HighlandsSchedule.Run(scope.ServiceProvider);
    else if(args.Contains("--seed-paired-fleet"))await PairedFleetSchedule.Run(scope.ServiceProvider);
    else if(args.Contains("--complete-user-profiles"))await DemoUserProfiles.Run(scope.ServiceProvider);
    else {await db.Database.OpenConnectionAsync();var pending=(await db.Database.GetPendingMigrationsAsync()).ToArray();Console.WriteLine($"SQL Server connected. Pending migrations: {pending.Length}");if(pending.Length>0)Environment.ExitCode=2;}
    return;
}
app.UseExceptionHandler(handler=>handler.Run(async ctx=> {
    var error=ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var sql=error as Microsoft.Data.SqlClient.SqlException??error?.InnerException as Microsoft.Data.SqlClient.SqlException;
    var status=sql==null?500:503;
    var message=sql==null?"Chưa thể hoàn tất yêu cầu. Vui lòng thử lại sau.":"Dịch vụ tạm thời chưa sẵn sàng. Vui lòng thử lại sau.";
    app.Logger.LogError(error,"Unhandled request {RequestId}",ctx.TraceIdentifier);
    ctx.Response.StatusCode=status;
    if(ctx.Request.Path.StartsWithSegments("/api"))await ctx.Response.WriteAsJsonAsync(new ProblemVm(status,message,ctx.TraceIdentifier));
    else {ctx.Response.ContentType="text/html; charset=utf-8";await ctx.Response.WriteAsync($"<!doctype html><html lang='vi'><head><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'><title>Bus Center · Thông báo</title><link rel='icon' href='/images/favicon.svg'><style>body{{margin:0;background:#f3f8f5;color:#153b3c;font:16px/1.7 system-ui,sans-serif}}main{{max-width:620px;margin:12vh auto;padding:32px;background:white;border:1px solid #dce8e1;border-radius:20px}}a{{color:#087f73}}small{{color:#576b65}}h1{{line-height:1.2}}@media(max-width:700px){{main{{margin:24px 16px;padding:24px}}}}</style></head><body><main><strong>Bus Center</strong><h1>Chưa thể hoàn tất yêu cầu</h1><p>{message}</p><p><small>Mã {status} · Mã yêu cầu: {System.Net.WebUtility.HtmlEncode(ctx.TraceIdentifier)}</small></p><a href='/'>Về trang chủ</a></main></body></html>");}
}));
if(!app.Environment.IsDevelopment()){app.UseHsts();app.UseHttpsRedirection();}
app.Use(async (ctx,next)=> {
    ctx.Response.Headers["X-Content-Type-Options"]="nosniff";ctx.Response.Headers["X-Frame-Options"]="DENY";
    ctx.Response.Headers["Referrer-Policy"]="strict-origin-when-cross-origin";
    await next();
});
app.UseStaticFiles();app.UseRouting();app.UseRateLimiter();app.UseAuthentication();
app.Use(async(ctx,next)=>{if(ctx.User.Identity?.IsAuthenticated==true||ctx.Request.Path.StartsWithSegments("/api")||!HttpMethods.IsGet(ctx.Request.Method))ctx.Response.Headers.CacheControl="no-store";await next();});
app.UseAuthorization();
app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
app.MapHub<SeatsHub>("/hubs/seats");
app.Run();
public partial class Program {}
