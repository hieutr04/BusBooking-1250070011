using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using BusBooking.Data;
using BusBooking.Models;
using BusBooking.Services;
using BusBooking.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

if(args.Contains("--planner-only")){StopAppendPlannerTests.Run();StopChoicePolicyTests.Run();return;}

var connection=Environment.GetEnvironmentVariable("SQL_TEST_CONNECTION")??"";
if(!args.Contains("--ack-test-db")||string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("Cần SQL_TEST_CONNECTION và --ack-test-db. Chỉ dùng database riêng kết thúc bằng _Tests.");
var cs=new SqlConnectionStringBuilder(connection);
if(!cs.InitialCatalog.EndsWith("_Tests",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Từ chối: tên database test phải kết thúc bằng _Tests. Không chạy trên DB học tập/người dùng.");
var services=new ServiceCollection();services.AddLogging(b=>b.AddSimpleConsole().SetMinimumLevel(LogLevel.Warning));
services.AddDbContext<AppDbContext>(o=>o.UseSqlServer(connection));
services.AddIdentity<AppUser,IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
services.AddSignalR();services.Configure<BookingOptions>(_=>{});services.AddSingleton<RealtimeMetrics>();
services.AddScoped<TripStopAppendService>();services.AddScoped<DbGate>();services.AddScoped<RealtimePublisher>();services.AddScoped<BookingService>();services.AddScoped<CatalogService>();services.AddScoped<ReportService>();
await using var provider=services.BuildServiceProvider();
var run=DateTime.UtcNow.ToString("yyyyMMddHHmmss")+Guid.NewGuid().ToString("N")[..5];var output=Path.GetFullPath(Path.Combine("TestResults",run));Directory.CreateDirectory(output);
var cases=new List<object>();var races=new List<object>();var json=new JsonSerializerOptions{WriteIndented=true,PropertyNameCaseInsensitive=true};
async Task<T> Use<T>(Func<IServiceProvider,Task<T>> work){using var scope=provider.CreateScope();return await work(scope.ServiceProvider);}
async Task Run(string name,Func<Task> test){var sw=Stopwatch.StartNew();try{await test();cases.Add(new{name,status="PASS",ms=sw.Elapsed.TotalMilliseconds});Console.WriteLine("PASS "+name);}catch(Exception e){cases.Add(new{name,status="FAIL",error=e.ToString()});Console.WriteLine("FAIL "+name+": "+e.Message);Environment.ExitCode=1;}}
void Assert(bool ok,string message){if(!ok)throw new Exception(message);}
async Task Conflict(Func<Task> action){try{await action();throw new Exception("Expected business conflict but action succeeded.");}catch(BusinessException e)when(e.Status is 400 or 403 or 409){}}
await Use(async s=>{var db=s.GetRequiredService<AppDbContext>();await db.Database.MigrateAsync();var rm=s.GetRequiredService<RoleManager<IdentityRole>>();foreach(var r in Roles.All)if(!await rm.RoleExistsAsync(r)){var result=await rm.CreateAsync(new(r));Assert(result.Succeeded,"Create role");}return 0;});
const string password="SqlTests!2026";
var actors=new List<(string Id,string Email)>();
for(int i=0;i<102;i++)actors.Add(await Use(async s=>{var um=s.GetRequiredService<UserManager<AppUser>>();var mail=$"sqltest-{run}-{i}@busbooking.test";var u=new AppUser{Email=mail,UserName=mail,FullName="Khách kiểm thử "+i,EmailConfirmed=true};var result=await um.CreateAsync(u,password);Assert(result.Succeeded,string.Join(";",result.Errors.Select(e=>e.Description)));Assert((await um.AddToRoleAsync(u,Roles.Customer)).Succeeded,"Assign Customer");return (u.Id,mail);}));
var staff=await Use(async s=>{var um=s.GetRequiredService<UserManager<AppUser>>();var mail=$"staff-{run}@busbooking.test";var u=new AppUser{Email=mail,UserName=mail,FullName="Nhân viên kiểm thử",EmailConfirmed=true};Assert((await um.CreateAsync(u,password)).Succeeded,"Create staff");Assert((await um.AddToRoleAsync(u,Roles.Staff)).Succeeded,"Role staff");return (u.Id,Email:mail);});
var driver=await Use(async s=>{var um=s.GetRequiredService<UserManager<AppUser>>();var mail=$"driver-{run}@busbooking.test";var u=new AppUser{Email=mail,UserName=mail,FullName="Tài xế kiểm thử",EmailConfirmed=true};Assert((await um.CreateAsync(u,password)).Succeeded,"Create driver");Assert((await um.AddToRoleAsync(u,Roles.Driver)).Succeeded,"Role driver");return (u.Id,Email:mail);});
var admin=await Use(async s=>{var um=s.GetRequiredService<UserManager<AppUser>>();var mail=$"admin-{run}@busbooking.test";var u=new AppUser{Email=mail,UserName=mail,FullName="Quản trị kiểm thử",EmailConfirmed=true};Assert((await um.CreateAsync(u,password)).Succeeded,"Create admin");Assert((await um.AddToRoleAsync(u,Roles.Admin)).Succeeded,"Role admin");return (u.Id,Email:mail);});
var otherDriver=await Use(async s=>{var um=s.GetRequiredService<UserManager<AppUser>>();var mail=$"driver2-{run}@busbooking.test";var u=new AppUser{Email=mail,UserName=mail,FullName="Tài xế chưa được giao chuyến",EmailConfirmed=true};Assert((await um.CreateAsync(u,password)).Succeeded,"Create other driver");Assert((await um.AddToRoleAsync(u,Roles.Driver)).Succeeded,"Role other driver");return (u.Id,Email:mail);});
int routeId=0,layoutId=0,vehicleId=0;
await Use(async s=>{var catalog=s.GetRequiredService<CatalogService>();var db=s.GetRequiredService<AppDbContext>();await catalog.SaveRoute("test",new RouteForm{Origin="Test-"+run,Destination="Đích-"+run,DurationMinutes=120});routeId=await db.Routes.Where(x=>x.Origin=="Test-"+run).Select(x=>x.Id).SingleAsync();await catalog.SaveLayout("test",new LayoutForm{Name="SQL fixture "+run,Rows=4,Columns=4,Floors=1});layoutId=await db.Layouts.Where(x=>x.Name=="SQL fixture "+run).Select(x=>x.Id).SingleAsync();await catalog.SaveVehicle("test",new VehicleForm{Plate="TEST-"+run,Name="SQL test fixture",SeatLayoutId=layoutId});vehicleId=await db.Vehicles.Where(x=>x.Plate=="TEST-"+run).Select(x=>x.Id).SingleAsync();return 0;});
int tripOrdinal=0;
async Task<int> NewTrip(){var local=DateTime.UtcNow.AddHours(7).Date.AddDays(20+Interlocked.Increment(ref tripOrdinal)).AddHours(8);return await Use(async s=>{await s.GetRequiredService<CatalogService>().SaveTrip("test",new TripForm{BusRouteId=routeId,VehicleId=vehicleId,DepartureLocal=local,ArrivalLocal=local.AddHours(2),Fare=150000,DriverId=driver.Id,StaffId=staff.Id});return await s.GetRequiredService<AppDbContext>().Trips.Where(x=>x.VehicleId==vehicleId&&x.DepartureUtc==BusinessTime.FromVietnam(local)).Select(x=>x.Id).SingleAsync();});}
async Task<int[]> SeatIds(int trip)=>await Use(s=>s.GetRequiredService<AppDbContext>().TripSeats.Where(x=>x.TripId==trip).OrderBy(x=>x.Id).Select(x=>x.Id).ToArrayAsync());
async Task<HoldView> Hold(string actor,int trip,int[] seats,Guid? key=null)=>await Use(s=>s.GetRequiredService<BookingService>().Acquire(actor,new HoldForm{TripId=trip,SeatIds=seats,RequestKey=key??Guid.NewGuid()}));
async Task<Guid> Confirm(string actor,HoldView hold,Guid? exchange=null,bool privileged=false)=>await Use(s=>s.GetRequiredService<BookingService>().Confirm(actor,privileged,new ConfirmForm{HoldId=hold.Id,ContactName="Liên hệ test",Phone="0900000000",Passengers=hold.SeatIds.Select(id=>new PassengerForm{TripSeatId=id,Name="Hành khách SQL test"}).ToList(),ExchangeBookingId=exchange}));
Task<int> Pay(string actor,Guid bookingId)=>Use(s=>s.GetRequiredService<BookingService>().SimulatePayment(actor,bookingId,default));
async Task Release(string actor,Guid id)=>await Use(async s=>{await s.GetRequiredService<BookingService>().Release(actor,id);return 0;});
async Task ExpireFixture(Guid id)=>await Use(async s=>await s.GetRequiredService<AppDbContext>().Database.ExecuteSqlInterpolatedAsync($"UPDATE dbo.SeatHolds SET CreatedUtc=DATEADD(second,-301,SYSUTCDATETIME()), ExpiresAtUtc=DATEADD(second,-1,SYSUTCDATETIME()) WHERE Id={id}"));
async Task DbInvariant(int trip){await Use(async s=>{var db=s.GetRequiredService<AppDbContext>();var duplicates=await db.Tickets.Where(x=>x.Seat.TripId==trip&&x.IsActive).GroupBy(x=>x.TripSeatId).Where(g=>g.Count()>1).CountAsync();Assert(duplicates==0,"double booking");var mismatch=await db.TripSeats.CountAsync(x=>x.TripId==trip&&((x.Status==SeatStatus.Booked&&!db.Tickets.Any(t=>t.TripSeatId==x.Id&&t.IsActive))||(x.Status!=SeatStatus.Booked&&db.Tickets.Any(t=>t.TripSeatId==x.Id&&t.IsActive))));Assert(mismatch==0,"Ticket/seat mismatch");return 0;});}
var browserTrip=await NewTrip();var browserSeats=await SeatIds(browserTrip);
var fixture=new{run,database=cs.InitialCatalog,tripId=browserTrip,seatIds=browserSeats,customers=actors.Take(3).Select(x=>new{x.Email,password}),staff=new{staff.Email,password},driver=new{driver.Email,password},otherDriver=new{otherDriver.Email,password},admin=new{admin.Email,password},output};
await File.WriteAllTextAsync(Path.Combine(output,"fixtures.json"),JsonSerializer.Serialize(fixture,json));
if(args.Contains("--fixtures-only")){Console.WriteLine("Fixtures: "+Path.Combine(output,"fixtures.json"));return;}
await Run("Qr_completion_is_idempotent_and_revenue_uses_payment_day",async()=>{
    var tripId=await NewTrip();
    var seatId=(await SeatIds(tripId))[0];
    var hold=await Hold(actors[0].Id,tripId,[seatId]);
    var bookingId=await Confirm(actors[0].Id,hold);
    var today=await Use(async s=>BusinessTime.Vietnam(await s.GetRequiredService<DbGate>().Now()).Date);
    await Use(async s=>await s.GetRequiredService<AppDbContext>().Database.ExecuteSqlInterpolatedAsync(
        $"UPDATE dbo.Bookings SET CreatedUtc={BusinessTime.FromVietnam(today.AddDays(-1).AddHours(12))} WHERE Id={bookingId}"));
    var before=await Use(s=>s.GetRequiredService<ReportService>().Dashboard(today,today));
    await Conflict(()=>Use(s=>s.GetRequiredService<BookingService>().SimulatePayment(actors[1].Id,bookingId,default)));
    var attempts=Enumerable.Range(0,3).Select(_=>Use(s=>
        s.GetRequiredService<BookingService>().SimulatePayment(actors[0].Id,bookingId,default)));
    await Task.WhenAll(attempts);
    await Use(async s=>{
        var db=s.GetRequiredService<AppDbContext>();
        var booking=await db.Bookings.Include(b=>b.Payments).SingleAsync(b=>b.Id==bookingId);
        Assert(booking.Payments.Count==1,"Repeated completion created multiple receipts");
        Assert(booking.Balance==booking.Total&&booking.SimulatedPaidAmount==booking.Total,"Payment did not settle the booking");
        Assert(booking.SimulatedPaidUtc.HasValue&&BusinessTime.Vietnam(booking.SimulatedPaidUtc.Value).Date==today,
            "Payment used the wrong Vietnam date");
        Assert(await db.AuditEntries.CountAsync(a=>a.Action=="QrPaymentCompleted"&&a.EntityId==bookingId.ToString())==1,
            "Repeated completion created multiple audit records");
        return 0;
    });
    var after=await Use(s=>s.GetRequiredService<ReportService>().Dashboard(today,today));
    Assert(after.Receipts-before.Receipts==150000,"Dashboard did not count payment on its completion day");
    Assert(after.Tickets==before.Tickets,"Payment date changed booking-created statistics");
    await Use(async s=>{await s.GetRequiredService<BookingService>().Cancel(staff.Id,bookingId);return 0;});
    await Use(async s=>{await s.GetRequiredService<BookingService>().Money(staff.Id,new PaymentForm{
        BookingId=bookingId,RequestKey=Guid.NewGuid(),Refund=true
    });return 0;});
    var refunded=await Use(s=>s.GetRequiredService<ReportService>().Dashboard(today,today));
    Assert(refunded.Refunds-before.Refunds==150000,"Cancelled QR payment was not refundable");
});
await Run("Abandoned_QR_releases_seats_without_receipt",async()=>{
    var trip=await NewTrip();
    var seats=await SeatIds(trip);
    var hold=await Hold(actors[0].Id,trip,seats[..2]);
    var booking=await Confirm(actors[0].Id,hold);
    var paid=await Confirm(actors[1].Id,await Hold(actors[1].Id,trip,[seats[2]]));
    await Pay(actors[1].Id,paid);
    var before=await Use(s=>s.GetRequiredService<BookingService>().Snapshot(trip,null));
    Assert(before.Seats.Single(s=>s.Id==seats[0]).Status=="Held","Awaiting QR appeared booked");
    Assert(before.Seats.Single(s=>s.Id==seats[2]).Status=="Booked","Paid seat did not appear booked");
    await Conflict(()=>Hold(actors[2].Id,trip,[seats[0]]));
    await Use(s=>s.GetRequiredService<AppDbContext>().Database.ExecuteSqlInterpolatedAsync(
        $"UPDATE dbo.Bookings SET PaymentExpiresUtc=DATEADD(second,-1,SYSUTCDATETIME()) WHERE Id={booking}"));
    await Conflict(()=>Pay(actors[0].Id,booking));
    Assert(await Use(s=>s.GetRequiredService<BookingService>().ExpireTrip(trip))==1,"Expiry did not process one booking");
    Assert(await Use(s=>s.GetRequiredService<BookingService>().ExpireTrip(trip))==0,"Expiry was not idempotent");
    var after=await Use(s=>s.GetRequiredService<BookingService>().Snapshot(trip,null));
    Assert(after.Revision>before.Revision,"Expiry did not advance seat map revision");
    Assert(after.Seats.Where(s=>seats[..2].Contains(s.Id)).All(s=>s.Status=="Available"),"Expired QR did not release all seats");
    Assert(after.Seats.Single(s=>s.Id==seats[2]).Status=="Booked","Expiry released a paid seat");
    await Conflict(()=>Pay(actors[0].Id,booking));
    await Hold(actors[2].Id,trip,[seats[0]]);
    await Use(async s=>{
        var db=s.GetRequiredService<AppDbContext>();
        var order=await db.Bookings.Include(x=>x.Payments).Include(x=>x.Tickets).SingleAsync(x=>x.Id==booking);
        Assert(order.Status==BookingStatus.Expired&&order.Payments.Count==0&&order.Tickets.All(t=>!t.IsActive),
            "Expired order still had an active ticket or receipt");
        return 0;
    });
    await DbInvariant(trip);
});
await Run("Independent_trip_seats",async()=>{var t1=await NewTrip();var t2=await NewTrip();var a=(await SeatIds(t1))[0];var b=(await SeatIds(t2))[0];var h1=await Hold(actors[0].Id,t1,[a]);var h2=await Hold(actors[1].Id,t2,[b]);Assert(h1.Id!=h2.Id,"Independent holds");await Release(actors[0].Id,h1.Id);await Release(actors[1].Id,h2.Id);});
await Run("Five_minute_configuration_and_retry_no_extension",async()=>{var t=await NewTrip();var ids=await SeatIds(t);var key=Guid.NewGuid();var h=await Hold(actors[0].Id,t,[ids[0]],key);var repeated=await Hold(actors[0].Id,t,[ids[0]],key);Assert(h.Id==repeated.Id&&h.ExpiresAtUtc==repeated.ExpiresAtUtc,"Retry changed hold");await Use(async s=>{var row=await s.GetRequiredService<AppDbContext>().SeatHolds.SingleAsync(x=>x.Id==h.Id);Assert((row.ExpiresAtUtc-row.CreatedUtc).TotalSeconds==300,"Hold not 300s");return 0;});await Release(actors[0].Id,h.Id);});
await Run("Hold_confirm_idempotency_and_no_expiry_of_booked",async()=>{var t=await NewTrip();var h=await Hold(actors[0].Id,t,[(await SeatIds(t))[0]]);var id=await Confirm(actors[0].Id,h);Assert(id==await Confirm(actors[0].Id,h),"Duplicate confirmation");await ExpireFixture(h.Id);await Use(async s=>{await s.GetRequiredService<BookingService>().ExpireTrip(t);return 0;});await DbInvariant(t);await Use(async s=>{Assert(await s.GetRequiredService<AppDbContext>().Tickets.CountAsync(x=>x.BookingId==id&&x.IsActive)==1,"Booked released after hold expiry");return 0;});});
await Run("Expiry_ownership_stale_tokens_and_reclaim_without_worker",async()=>{var t=await NewTrip();var seat=(await SeatIds(t))[0];var h=await Hold(actors[0].Id,t,[seat]);await Conflict(()=>Confirm(actors[1].Id,h));await Conflict(()=>Release(actors[1].Id,h.Id));await ExpireFixture(h.Id);var b=await Hold(actors[1].Id,t,[seat]);await Conflict(()=>Confirm(actors[0].Id,h));await Release(actors[0].Id,h.Id);await Use(async s=>{var row=await s.GetRequiredService<AppDbContext>().TripSeats.SingleAsync(x=>x.Id==seat);Assert(row.CurrentHoldId==b.Id,"Old release overwrote B hold");return 0;});await Confirm(actors[1].Id,b);await DbInvariant(t);});
await Run("Stops_validate_persist_restore_snapshot_and_retry",async()=>{
 var t=await NewTrip();var other=await NewTrip();
 await Use(async sp=>{
  var db=sp.GetRequiredService<AppDbContext>();var gate=sp.GetRequiredService<DbGate>();var service=new TripStopService(db,gate);
  var f=await service.Edit(t);f.Stops.Insert(1,new StopEditRow{Name="Điểm giữa học tập",OffsetMinutes=60,CanPickup=true,CanDropoff=true});await service.Save(admin.Id,f);return 0;
 });
 var points=await Use(sp=>sp.GetRequiredService<AppDbContext>().TripStops.AsNoTracking().Where(x=>x.TripId==t).OrderBy(x=>x.StopOrder).ToListAsync());
 var foreign=await Use(sp=>sp.GetRequiredService<AppDbContext>().TripStops.Where(x=>x.TripId==other).OrderBy(x=>x.StopOrder).Select(x=>x.Id).FirstAsync());
 var seat=(await SeatIds(t))[0];
 await Conflict(()=>Use(sp=>sp.GetRequiredService<BookingService>().Acquire(actors[0].Id,new HoldForm{TripId=t,SeatIds=[seat],RequestKey=Guid.NewGuid(),PickupStopId=foreign,DropoffStopId=points[2].Id})));
 await Conflict(()=>Use(sp=>sp.GetRequiredService<BookingService>().Acquire(actors[0].Id,new HoldForm{TripId=t,SeatIds=[seat],RequestKey=Guid.NewGuid(),PickupStopId=points[1].Id,DropoffStopId=points[0].Id})));
 var h=await Use(sp=>sp.GetRequiredService<BookingService>().Acquire(actors[0].Id,new HoldForm{TripId=t,SeatIds=[seat],RequestKey=Guid.NewGuid(),PickupStopId=points[1].Id,DropoffStopId=points[2].Id}));
 var updated=await Use(sp=>sp.GetRequiredService<BookingService>().UpdateStops(actors[0].Id,new HoldStopsForm{HoldId=h.Id,PickupStopId=points[0].Id,DropoffStopId=points[2].Id}));
 Assert(updated.ExpiresAtUtc==h.ExpiresAtUtc,"Changing stops extended hold");
 await Conflict(()=>Use(sp=>sp.GetRequiredService<BookingService>().UpdateStops(actors[1].Id,new HoldStopsForm{HoldId=h.Id,PickupStopId=points[1].Id,DropoffStopId=points[2].Id})));
 var snap=await Use(sp=>sp.GetRequiredService<BookingService>().Snapshot(t,actors[0].Id));
 Assert(snap.MyHolds.Single().PickupStopId==points[0].Id&&snap.Stops.Count==3,"Reload lost stop IDs");
 await Conflict(async()=>{await Use(async sp=>{var service=new TripStopService(sp.GetRequiredService<AppDbContext>(),sp.GetRequiredService<DbGate>());var f=await service.Edit(t);Assert(f.Locked,"Schedule not locked after hold");await service.Save(admin.Id,f);return 0;});});
 var id=await Confirm(actors[0].Id,updated);Assert(await Confirm(actors[0].Id,updated)==id,"Confirm retry duplicated booking");
 await Conflict(()=>Use(sp=>sp.GetRequiredService<BookingService>().Confirm(actors[0].Id,false,new ConfirmForm{HoldId=h.Id,PickupStopId=points[1].Id,DropoffStopId=points[2].Id})));
 await Use(async sp=>{var db=sp.GetRequiredService<AppDbContext>();var b=await db.Bookings.SingleAsync(x=>x.Id==id);var tr=await db.Trips.SingleAsync(x=>x.Id==t);
  Assert(b.PickupName==points[0].Name&&b.DropoffName==points[2].Name&&b.PickupAtUtc==tr.DepartureUtc&&b.DropoffAtUtc==tr.ArrivalUtc,"Booking snapshot wrong");
  Assert(b.Total==150000,"Stop choice changed full-trip fare");return 0;});await DbInvariant(t);
});
await Run("Specific_endpoints_preserve_old_holds_bookings_and_change_new_defaults_only",async()=>{
 var t=await NewTrip();var ids=await SeatIds(t);var bookedHold=await Hold(actors[0].Id,t,[ids[0]]);var booked=await Confirm(actors[0].Id,bookedHold);
 var active=await Hold(actors[1].Id,t,[ids[1]]);var expired=await Hold(actors[2].Id,t,[ids[2]]);await ExpireFixture(expired.Id);
 var pair=await Use(async sp=>{
  var db=sp.GetRequiredService<AppDbContext>();var trip=await db.Trips.AsNoTracking().Include(x=>x.Route).SingleAsync(x=>x.Id==t);
  async Task<string> Historical()=>JsonSerializer.Serialize(new {
   stops=await db.TripStops.AsNoTracking().Where(x=>x.TripId==t).OrderBy(x=>x.Id).ToListAsync(),
   holds=await db.SeatHolds.AsNoTracking().Where(x=>x.TripId==t).OrderBy(x=>x.Id).Select(x=>new{x.Id,x.OwnerId,x.CreatedUtc,x.ExpiresAtUtc,x.Status,x.PickupStopId,x.DropoffStopId,x.RequestKey}).ToListAsync(),
   booking=await db.Bookings.AsNoTracking().Where(x=>x.Id==booked).Select(x=>new{x.Id,x.Total,x.Status,x.PickupName,x.DropoffName,x.PickupAddress,x.DropoffAddress,x.PickupAtUtc,x.DropoffAtUtc,x.PickupStopId,x.DropoffStopId,x.StopsIllustrative}).SingleAsync()
  });
  var originalStops=JsonSerializer.Serialize(await db.TripStops.AsNoTracking().Where(x=>x.TripId==t).OrderBy(x=>x.Id).ToListAsync());
  var before=await Historical();var oldIds=await db.TripStops.Where(x=>x.TripId==t).Select(x=>x.Id).ToArrayAsync();
  var holds=JsonSerializer.Serialize(await db.SeatHolds.AsNoTracking().Where(x=>x.TripId==t).OrderBy(x=>x.Id).Select(x=>new{x.Id,x.OwnerId,x.CreatedUtc,x.ExpiresAtUtc,x.Status,x.PickupStopId,x.DropoffStopId,x.RequestKey}).ToListAsync());
  var requests=new SpecificStopRequest[]{new("Điểm đón SQL fixture","Địa chỉ giả dành riêng cho test",0,true,false,trip.Route.Origin,"Test fixture operator review",true),new("Điểm trả SQL fixture","Địa chỉ giả dành riêng cho test",120,false,true,trip.Route.Destination,"Test fixture operator review",true)};
  var service=sp.GetRequiredService<TripStopAppendService>();var dry=await service.Specific(admin.Id,t,trip.Revision,requests,true);Assert(dry.Plan.Ready&&!dry.Applied&&before==await Historical(),"dry-run wrote data");
  var result=await service.Specific(admin.Id,t,trip.Revision,requests);Assert(result.Applied&&result.AfterRevision==trip.Revision+1,"specific append/revision");
  Assert(holds==JsonSerializer.Serialize(await db.SeatHolds.AsNoTracking().Where(x=>x.TripId==t).OrderBy(x=>x.Id).Select(x=>new{x.Id,x.OwnerId,x.CreatedUtc,x.ExpiresAtUtc,x.Status,x.PickupStopId,x.DropoffStopId,x.RequestKey}).ToListAsync()),"hold or expiry changed");
  var afterOld=await db.TripStops.AsNoTracking().Where(x=>x.TripId==t&&oldIds.Contains(x.Id)).OrderBy(x=>x.Id).ToListAsync();
  Assert(originalStops==JsonSerializer.Serialize(afterOld),"old point ID/order/name/address/flags/time changed");
  var again=await service.Specific(admin.Id,t,result.AfterRevision,requests);Assert(!again.Applied&&again.Plan.Status=="complete"&&again.AfterRevision==result.AfterRevision,"repeat changed data");
  var sourceIds=result.Plan.NewStops.Select(z=>z.Id.ToString()).ToArray();
  Assert(await db.AuditEntries.CountAsync(x=>x.Action=="SpecificStopSource"&&sourceIds.Contains(x.EntityId))==2,"source audit missing/duplicated");
  var rows=await db.TripStops.AsNoTracking().Where(x=>x.TripId==t).ToListAsync();var next=StopChoicePolicy.SelectNew(trip,trip.Route,rows,null,null);
  var booking=await db.Bookings.AsNoTracking().SingleAsync(x=>x.Id==booked);Assert(booking.PickupStopId==bookedHold.PickupStopId&&booking.DropoffStopId==bookedHold.DropoffStopId&&booking.Total==150000,"historical booking changed");
  return (Pickup:next.Pickup.Id,Dropoff:next.Dropoff.Id);
 });
 await Conflict(async()=>{await Use(sp=>sp.GetRequiredService<BookingService>().Acquire(actors[3].Id,new HoldForm{TripId=t,SeatIds=[ids[3]],RequestKey=Guid.NewGuid(),PickupStopId=active.PickupStopId,DropoffStopId=active.DropoffStopId}));});
 var snap=await Use(sp=>sp.GetRequiredService<BookingService>().Snapshot(t,actors[1].Id));Assert(snap.MyHolds.Single().ExpiresAtUtc==active.ExpiresAtUtc&&snap.Stops.Any(x=>x.Id==active.PickupStopId&&!x.CanSelect),"holder choice not retained");
 await Confirm(actors[1].Id,active);var fresh=await Hold(actors[3].Id,t,[ids[3]]);Assert(fresh.PickupStopId==pair.Pickup&&fresh.DropoffStopId==pair.Dropoff,"new hold not using configured endpoints");await Confirm(actors[3].Id,fresh);await DbInvariant(t);
});
await Run("Dalat_existing_held_trip_dry_run_and_concurrent_completion_are_idempotent",async()=>{
 var local=DateTime.UtcNow.AddHours(7).Date.AddDays(20+Interlocked.Increment(ref tripOrdinal)).AddHours(23);
 var t=await Use(async sp=>{
  var db=sp.GetRequiredService<AppDbContext>();var catalog=sp.GetRequiredService<CatalogService>();
  var route=await db.Routes.SingleOrDefaultAsync(x=>x.Origin=="Đà Lạt"&&x.Destination=="TP. Hồ Chí Minh");
  if(route==null){await catalog.SaveRoute(admin.Id,new RouteForm{Origin="Đà Lạt",Destination="TP. Hồ Chí Minh",DurationMinutes=420});route=await db.Routes.SingleAsync(x=>x.Origin=="Đà Lạt"&&x.Destination=="TP. Hồ Chí Minh");}
  await catalog.SaveTrip(admin.Id,new TripForm{BusRouteId=route.Id,VehicleId=vehicleId,DepartureLocal=local,ArrivalLocal=local.AddHours(7),Fare=320000,DriverId=driver.Id,StaffId=staff.Id});
  var trip=await db.Trips.SingleAsync(x=>x.VehicleId==vehicleId&&x.DepartureUtc==BusinessTime.FromVietnam(local));
  Assert(await db.TripStops.CountAsync(x=>x.TripId==trip.Id)==4,"new trip defaults missing");
  var editor=new TripStopService(db,sp.GetRequiredService<DbGate>());var form=await editor.Edit(trip.Id);
  form.Stops=form.Stops.Where(x=>x.OffsetMinutes==0||x.OffsetMinutes==420).ToList();await editor.Save(admin.Id,form);return trip.Id;
 });
 var hold=await Hold(actors[0].Id,t,[(await SeatIds(t))[0]]);
 var before=await Use(async sp=>{var db=sp.GetRequiredService<AppDbContext>();return (Revision:(await db.Trips.AsNoTracking().SingleAsync(x=>x.Id==t)).Revision,Stops:JsonSerializer.Serialize(await db.TripStops.AsNoTracking().Where(x=>x.TripId==t).OrderBy(x=>x.Id).ToListAsync()));});
 await Use(async sp=>{var result=await sp.GetRequiredService<TripStopAppendService>().CompleteDalat(admin.Id,t,before.Revision,true);Assert(result.Plan.Ready&&!result.Applied&&result.Plan.NewStops.Count==2&&result.AfterRevision==before.Revision,"dry-run wrote or missing points");return 0;});
 var barrier=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
 var attempts=Enumerable.Range(0,2).Select(_=>Task.Run(async()=>{await barrier.Task;return await Use(sp=>sp.GetRequiredService<TripStopAppendService>().CompleteDalat(admin.Id,t,null));})).ToArray();barrier.SetResult(true);
 var results=await Task.WhenAll(attempts);Assert(results.Count(x=>x.Applied)==1&&results.Count(x=>x.Plan.Status=="complete")==1,"concurrent duplicate append");
 await Use(async sp=>{
  var db=sp.GetRequiredService<AppDbContext>();var rows=await db.TripStops.AsNoTracking().Where(x=>x.TripId==t).OrderBy(x=>x.StopOrder).ToListAsync();
  Assert(rows.Count==4&&rows.Select(x=>x.OffsetMinutes).SequenceEqual(new[]{0,90,180,420}),"wrong direction/order/time");
  Assert(before.Stops==JsonSerializer.Serialize(rows.Where(x=>x.OffsetMinutes==0||x.OffsetMinutes==420).OrderBy(x=>x.Id).ToList()),"old endpoints modified");
  var current=await db.SeatHolds.AsNoTracking().SingleAsync(x=>x.Id==hold.Id);Assert(current.ExpiresAtUtc==hold.ExpiresAtUtc&&current.PickupStopId==hold.PickupStopId&&current.DropoffStopId==hold.DropoffStopId,"hold changed/extended");
  Assert((await db.Trips.AsNoTracking().SingleAsync(x=>x.Id==t)).Revision==before.Revision+1,"wrong revision");return 0;
 });
 var booking=await Confirm(actors[0].Id,hold);await Use(async sp=>{var b=await sp.GetRequiredService<AppDbContext>().Bookings.AsNoTracking().SingleAsync(x=>x.Id==booking);Assert(b.Total==320000&&b.PickupStopId==hold.PickupStopId&&b.DropoffStopId==hold.DropoffStopId,"confirmation changed selection or fare");return 0;});await DbInvariant(t);
});
await Run("Append_with_active_expired_and_confirmed_holds_is_insert_only_and_idempotent",async()=>{
 var t=await NewTrip();var ids=await SeatIds(t);
 var confirmed=await Hold(actors[0].Id,t,[ids[0]]);await Confirm(actors[0].Id,confirmed);
 var active=await Hold(actors[1].Id,t,[ids[1]]);var expired=await Hold(actors[2].Id,t,[ids[2]]);await ExpireFixture(expired.Id);
 await Use(async sp=>{
  var db=sp.GetRequiredService<AppDbContext>();var gate=sp.GetRequiredService<DbGate>();
  async Task<string> Commitments()=>JsonSerializer.Serialize(new {
   holds=await db.SeatHolds.AsNoTracking().Where(x=>x.TripId==t).OrderBy(x=>x.Id).Select(x=>new{x.Id,x.OwnerId,x.RequestKey,x.PickupStopId,x.DropoffStopId,x.CreatedUtc,x.ExpiresAtUtc,x.Status}).ToListAsync(),
   seats=await db.TripSeats.AsNoTracking().Where(x=>x.TripId==t).OrderBy(x=>x.Id).Select(x=>new{x.Id,x.PhysicalSeatId,x.Code,x.Row,x.Column,x.Floor,x.Status,x.CurrentHoldId}).ToListAsync(),
   bookings=await db.Bookings.AsNoTracking().Where(x=>x.Hold.TripId==t).OrderBy(x=>x.Id).Select(x=>new{x.Id,x.Total,x.Status,x.PickupStopId,x.DropoffStopId,x.PickupName,x.DropoffName,x.PickupAddress,x.DropoffAddress,x.PickupAtUtc,x.DropoffAtUtc,x.StopsIllustrative}).ToListAsync(),
   tickets=await db.Tickets.AsNoTracking().Where(x=>x.Seat.TripId==t).OrderBy(x=>x.Id).Select(x=>new{x.Id,x.BookingId,x.TripSeatId,x.PassengerName,x.Fare,x.IsActive}).ToListAsync()
  });
  var before=await Commitments();var old=await db.TripStops.AsNoTracking().Where(x=>x.TripId==t).OrderBy(x=>x.Id).ToListAsync();var oldJson=JsonSerializer.Serialize(old);var oldIds=old.Select(x=>x.Id).ToArray();
  var geometry=await db.Trips.AsNoTracking().SingleAsync(x=>x.Id==t);
  var service=sp.GetRequiredService<TripStopAppendService>();
  var first=await service.Append(admin.Id,t,geometry.Revision,new("Điểm riêng",null,60));Assert(first.Applied&&first.AfterRevision==first.BeforeRevision+1,"not appended");
  Assert(before==await Commitments(),"hold/expiry/seat/booking/ticket changed");
  Assert(oldJson==JsonSerializer.Serialize(await db.TripStops.AsNoTracking().Where(x=>x.TripId==t&&oldIds.Contains(x.Id)).OrderBy(x=>x.Id).ToListAsync()),"old stops edited");
  var again=await service.Append(admin.Id,t,first.AfterRevision,new("Điểm riêng",null,60));Assert(!again.Applied&&again.Plan.Status=="complete"&&again.AfterRevision==first.AfterRevision,"second run duplicates or bumps revision");
  await Conflict(async()=>{await service.Append(admin.Id,t,geometry.Revision,new("Điểm khác",null,70));});
  var editor=new TripStopService(db,gate);var form=await editor.Edit(t);Assert(form.Locked,"full edit unlocked despite holds");await Conflict(()=>editor.Save(admin.Id,form));
  var after=await db.Trips.AsNoTracking().SingleAsync(x=>x.Id==t);
  Assert(geometry.BusRouteId==after.BusRouteId&&geometry.VehicleId==after.VehicleId&&geometry.DepartureUtc==after.DepartureUtc&&geometry.ArrivalUtc==after.ArrivalUtc&&geometry.Fare==after.Fare&&geometry.DriverId==after.DriverId&&geometry.StaffId==after.StaffId,"trip geometry changed");
  Assert(await db.AuditEntries.CountAsync(x=>x.Action=="TripStopsAppended"&&x.EntityId==t.ToString())==1,"wrong audit count");
  return 0;
 });
 await Release(actors[1].Id,active.Id);await DbInvariant(t);
});
await Run("Stops_endpoint_update_is_idempotent_and_preserves_trip_geometry",async()=>{
 var t=await NewTrip();
 await Use(async sp=>{var db=sp.GetRequiredService<AppDbContext>();var before=await db.Trips.Include(x=>x.Route).AsNoTracking().SingleAsync(x=>x.Id==t);var ids=await db.TripStops.Where(x=>x.TripId==t).Select(x=>x.Id).Order().ToArrayAsync();
  await using var tx=await db.Database.BeginTransactionAsync();var trip=await sp.GetRequiredService<DbGate>().Trip(t);
  await TripStopService.EnsureEndpoints(db,trip,before.Route);await TripStopService.EnsureEndpoints(db,trip,before.Route);await tx.CommitAsync();
  var after=await db.Trips.AsNoTracking().SingleAsync(x=>x.Id==t);var next=await db.TripStops.Where(x=>x.TripId==t).Select(x=>x.Id).Order().ToArrayAsync();
  Assert(ids.SequenceEqual(next)&&before.DepartureUtc==after.DepartureUtc&&before.ArrivalUtc==after.ArrivalUtc&&before.VehicleId==after.VehicleId&&before.Fare==after.Fare,"Endpoint update changed existing trip or duplicated points");return 0;});
});
await Run("Stops_UTC_cross_midnight_and_reverse_teaching_template",()=>{
 var tr=new Trip{Id=1,DepartureUtc=new DateTime(2026,11,4,16,0,0,DateTimeKind.Utc),ArrivalUtc=new DateTime(2026,11,5,0,0,0,DateTimeKind.Utc)};
 var p=new TripStop{Id=1,TripId=1,StopOrder=0,OffsetMinutes=0,CanPickup=true};var d=new TripStop{Id=2,TripId=1,StopOrder=1000,OffsetMinutes=480,CanDropoff=true};
 var pair=TripStopService.Select(tr,new[]{p,d},1,2);Assert(BusinessTime.Text(TripStopService.AtUtc(tr,p))=="04/11/2026 23:00","Pickup VN date wrong");
 Assert(BusinessTime.Text(TripStopService.AtUtc(tr,d))=="05/11/2026 07:00","Dropoff VN next date wrong");
 Assert(TripStopMaintenance.StudyTemplate("Đà Lạt","TP. Hồ Chí Minh",420)[0].Minutes==90,"Reverse offset wrong");return Task.CompletedTask;
});
await Run("Multi_seat_all_or_nothing",async()=>{var t=await NewTrip();var seats=await SeatIds(t);var h=await Hold(actors[0].Id,t,[seats[0]]);await Conflict(()=>Hold(actors[1].Id,t,[seats[1],seats[0]]));await Use(async s=>{Assert((await s.GetRequiredService<AppDbContext>().TripSeats.SingleAsync(x=>x.Id==seats[1])).Status==SeatStatus.Available,"Partial hold committed");return 0;});await Release(actors[0].Id,h.Id);});
await Run("Mismatched_trip_and_passenger_seats",async()=>{var t=await NewTrip();var other=await NewTrip();var otherSeat=(await SeatIds(other))[0];await Conflict(()=>Hold(actors[0].Id,t,[otherSeat]));var h=await Hold(actors[0].Id,t,[(await SeatIds(t))[0]]);await Conflict(async()=>{await Use(s=>s.GetRequiredService<BookingService>().Confirm(actors[0].Id,false,new ConfirmForm{HoldId=h.Id,ContactName="Test",Phone="0900000000",Passengers=[new PassengerForm{TripSeatId=otherSeat,Name="Sai ghế"}]}));});await Release(actors[0].Id,h.Id);});
await Run("Expired_confirm_and_worker_race",async()=>{var t=await NewTrip();var h=await Hold(actors[0].Id,t,[(await SeatIds(t))[0]]);await ExpireFixture(h.Id);await Task.WhenAll(Conflict(()=>Confirm(actors[0].Id,h)),Use(async s=>{await s.GetRequiredService<BookingService>().ExpireTrip(t);return 0;}));await DbInvariant(t);});
await Run("Staff_sale_links_customer_history",async()=>{
 var t=await NewTrip();var h=await Hold(staff.Id,t,[(await SeatIds(t))[0]]);
 var id=await Use(s=>s.GetRequiredService<BookingService>().Confirm(staff.Id,true,new ConfirmForm{HoldId=h.Id,CustomerEmail=actors[0].Email,ContactName="Khách mua tại phòng vé",Phone="0900000000",Passengers=h.SeatIds.Select(x=>new PassengerForm{TripSeatId=x,Name="Hành khách thử"}).ToList()}));
 await Use(async s=>{Assert((await s.GetRequiredService<AppDbContext>().Bookings.SingleAsync(x=>x.Id==id)).OwnerId==actors[0].Id,"Staff-assisted sale not linked to customer");return 0;});await DbInvariant(t);
});
await Run("Cancel_payment_refund_idempotency",async()=>{var t=await NewTrip();var h=await Hold(actors[0].Id,t,[(await SeatIds(t))[0]]);var id=await Confirm(actors[0].Id,h);var key=Guid.NewGuid();for(int i=0;i<2;i++)await Use(async s=>{await s.GetRequiredService<BookingService>().Money(staff.Id,new PaymentForm{BookingId=id,RequestKey=key});return 0;});for(int i=0;i<2;i++)await Use(async s=>{await s.GetRequiredService<BookingService>().Cancel(staff.Id,id);return 0;});var refund=Guid.NewGuid();for(int i=0;i<2;i++)await Use(async s=>{await s.GetRequiredService<BookingService>().Money(staff.Id,new PaymentForm{BookingId=id,RequestKey=refund,Refund=true});return 0;});await Use(async s=>{var b=await s.GetRequiredService<AppDbContext>().Bookings.Include(x=>x.Payments).SingleAsync(x=>x.Id==id);Assert(b.Payments.Count==2&&b.Balance==0&&b.Status==BookingStatus.Cancelled,"Payment or cancel retry duplicated");return 0;});await DbInvariant(t);});
await Run("Partial_payments_are_capped_and_retry_safe",async()=>{
    var trip=await NewTrip();
    var hold=await Hold(actors[0].Id,trip,[(await SeatIds(trip))[0]]);
    var booking=await Confirm(actors[0].Id,hold);
    var receiptKey=Guid.NewGuid();
    var refundKey=Guid.NewGuid();
    async Task Record(long? amount,Guid key,bool refund=false)=>await Use(async s=>{
        await s.GetRequiredService<BookingService>().Money(staff.Id,new PaymentForm{
            BookingId=booking,RequestKey=key,Amount=amount,Refund=refund
        });
        return 0;
    });
    await Record(40000,receiptKey);
    await Record(40000,receiptKey);
    await Conflict(()=>Record(30000,receiptKey));
    await Conflict(()=>Record(0,Guid.NewGuid()));
    await Conflict(()=>Record(110001,Guid.NewGuid()));
    await Conflict(()=>Record(10000,Guid.NewGuid(),true));
    await Use(async s=>{await s.GetRequiredService<BookingService>().Cancel(staff.Id,booking);return 0;});
    await Record(10000,refundKey,true);
    await Record(10000,refundKey,true);
    await Conflict(()=>Record(30001,Guid.NewGuid(),true));
    await Record(null,Guid.NewGuid(),true);
    await Use(async s=>{
        var row=await s.GetRequiredService<AppDbContext>().Bookings.Include(b=>b.Payments).SingleAsync(b=>b.Id==booking);
        Assert(row.Balance==0&&row.Payments.Select(p=>p.Amount).Order().SequenceEqual(new decimal[]{-30000,-10000,40000}),"Unexpected receipt or refund ledger");
        return 0;
    });
});
await Run("Exchange_rollback_and_success_preserves_old_history",async()=>{
    var trip=await NewTrip();
    var target=await NewTrip();
    var seat=(await SeatIds(trip))[0];
    var targetSeats=await SeatIds(target);
    var original=await Confirm(actors[0].Id,await Hold(actors[0].Id,trip,[seat]));
    await Pay(actors[0].Id,original);
    await Use(async s=>{
        await s.GetRequiredService<BookingService>().RequestChange(actors[0].Id,new RequestForm{
            BookingId=original,Kind=ChangeKind.Exchange,Reason="Đổi chuyến kiểm thử"});
        return 0;
    });
    var expiredHold=await Hold(staff.Id,target,[targetSeats[0]]);
    await ExpireFixture(expiredHold.Id);
    await Conflict(()=>Confirm(staff.Id,expiredHold,original,true));
    var pending=await Confirm(staff.Id,await Hold(staff.Id,target,[targetSeats[0]]),original,true);
    await Conflict(async()=>{await Confirm(staff.Id,await Hold(staff.Id,target,[targetSeats[1]]),original,true);});
    await Use(async s=>{
        var db=s.GetRequiredService<AppDbContext>();
        Assert((await db.Bookings.SingleAsync(b=>b.Id==original)).Status==BookingStatus.Active,"Unpaid replacement changed original");
        Assert(await db.Tickets.AnyAsync(t=>t.BookingId==original&&t.IsActive),"Original ticket became inactive");
        Assert(await db.ChangeRequests.AnyAsync(r=>r.BookingId==original&&r.Status==RequestStatus.Pending),"Request closed before payment");
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE dbo.Bookings SET PaymentExpiresUtc=DATEADD(second,-1,SYSUTCDATETIME()) WHERE Id={pending}");
        return 0;
    });
    await Conflict(()=>Pay(actors[0].Id,pending));
    await Use(s=>s.GetRequiredService<BookingService>().ExpireTrip(target));
    await Use(async s=>{
        var db=s.GetRequiredService<AppDbContext>();
        Assert(await db.Tickets.AnyAsync(t=>t.BookingId==original&&t.IsActive),"Expiry deactivated original ticket");
        Assert(await db.TripSeats.AnyAsync(x=>x.Id==targetSeats[0]&&x.Status==SeatStatus.Available),"Expiry did not release replacement seat");
        return 0;
    });
    var replacement=await Confirm(staff.Id,await Hold(staff.Id,target,[targetSeats[0]]),original,true);
    await Task.WhenAll(Pay(actors[0].Id,replacement),Pay(actors[0].Id,replacement));
    await Use(async s=>{
        var db=s.GetRequiredService<AppDbContext>();
        Assert(await db.Bookings.AnyAsync(b=>b.Id==original&&b.Status==BookingStatus.Exchanged),"Original did not change after payment");
        Assert(await db.TripSeats.AnyAsync(x=>x.Id==seat&&x.Status==SeatStatus.Available),"Original seat was not released after payment");
        Assert(await db.Bookings.AnyAsync(b=>b.Id==replacement&&b.OwnerId==actors[0].Id&&b.PaymentExpiresUtc==null),"Replacement owner or payment state incorrect");
        Assert(await db.Payments.CountAsync(p=>p.BookingId==replacement)==1,"Exchange payment was duplicated");
        Assert(await db.ChangeRequests.AnyAsync(r=>r.BookingId==original&&r.Status==RequestStatus.Approved),"Paid exchange did not resolve request");
        return 0;
    });
    await DbInvariant(trip);
    await DbInvariant(target);
});
await Run("Trip_batch_preview_create_retry_and_conflict",async()=>{
    var date=BusinessTime.Vietnam(DateTime.UtcNow).Date.AddDays(500);
    var form=new TripBatchForm {
        BusRouteId=routeId,VehicleId=vehicleId,From=date,To=date.AddDays(2),DepartureTime=new TimeOnly(8,0),
        Fare=150000,DriverId=driver.Id,CoDriverId=otherDriver.Id,StaffId=staff.Id
    };
    Task<IReadOnlyList<TripBatchRow>> Batch(bool create)=>Use(s=>s.GetRequiredService<CatalogService>().BatchTrips("test",form,create,default));
    Assert((await Batch(false)).All(r=>r.Ready&&r.TripId==null),"Preview should show 3 unsaved trips");
    var start=BusinessTime.FromVietnam(date);
    Assert(await Use(s=>s.GetRequiredService<AppDbContext>().Trips.CountAsync(t=>t.VehicleId==vehicleId&&t.DepartureUtc>=start))==0,"Preview wrote trips");
    var created=await Batch(true);
    Assert(created.Count==3&&created.All(r=>r.TripId!=null&&r.Status=="Đã tạo"),"Batch did not create expected trips");
    Assert((await Batch(true)).Select(r=>r.TripId).SequenceEqual(created.Select(r=>r.TripId)),"Repeated batch duplicated trips");
    form.DepartureTime=new TimeOnly(9,0);
    Assert((await Batch(false)).All(r=>r.Status=="Trùng lịch xe"),"Preview failed to flag overlaps");
    Assert((await Batch(true)).All(r=>r.TripId==null),"Batch created an overlapping trip");
    Assert((await SeatIds(created[0].TripId!.Value)).Length==16,"Batch trip did not initialize seat inventory");
});
await Run("Trip_close_races_confirm",async()=>{var t=await NewTrip();var h=await Hold(actors[0].Id,t,[(await SeatIds(t))[0]]);var confirm=Task.Run(async()=>{try{await Confirm(actors[0].Id,h);}catch(BusinessException e)when(e.Status==409){}});var close=Use(async s=>{await s.GetRequiredService<BookingService>().SetTripState(staff.Id,true,t,null,false);return 0;});await Task.WhenAll(confirm,close);await DbInvariant(t);var freeSeat=(await SeatIds(t))[1];await Conflict(()=>Hold(actors[1].Id,t,[freeSeat]));});
await Run("Trip_cancel_releases_booked_and_held",async()=>{var t=await NewTrip();var seats=await SeatIds(t);var h=await Hold(actors[0].Id,t,[seats[0]]);var id=await Confirm(actors[0].Id,h);await Hold(actors[1].Id,t,[seats[1]]);await Use(async s=>{await s.GetRequiredService<BookingService>().SetTripState(staff.Id,true,t,TripStatus.Cancelled,null);return 0;});await DbInvariant(t);await Use(async s=>{Assert((await s.GetRequiredService<AppDbContext>().Bookings.SingleAsync(x=>x.Id==id)).Status==BookingStatus.Cancelled,"Cancel trip not reflected");return 0;});});
await Run("Driver_ownership_and_progression",async()=>{var t=await NewTrip();await Conflict(async()=>{await Use(async s=>{await s.GetRequiredService<BookingService>().SetTripState(actors[0].Id,false,t,TripStatus.Boarding,null);return 0;});});await Conflict(async()=>{await Use(async s=>{await s.GetRequiredService<BookingService>().SetTripState(driver.Id,false,t,TripStatus.Completed,null);return 0;});});foreach(var status in new[]{TripStatus.Boarding,TripStatus.InProgress,TripStatus.Completed})await Use(async s=>{await s.GetRequiredService<BookingService>().SetTripState(driver.Id,false,t,status,null);return 0;});});
await Run("CheckIn_requires_assignment_and_records_one_scan",async()=>{
    var trip=await NewTrip();
    var wrongTrip=await NewTrip();
    var booking=await Confirm(actors[0].Id,await Hold(actors[0].Id,trip,[(await SeatIds(trip))[0]]));
    var ticket=await Use(s=>s.GetRequiredService<AppDbContext>().Tickets.SingleAsync(t=>t.BookingId==booking));
    var code=TicketCheckIn.Prefix+ticket.Id.ToString("N");
    await Pay(actors[0].Id,booking);
    Task<string> Scan(int tripId,string actor,bool driverRole)=>Use(s=>{
        var db=s.GetRequiredService<AppDbContext>();
        return new TicketCheckIn(db,s.GetRequiredService<DbGate>()).Record(tripId,code,actor,false,driverRole,default);
    });
    await Conflict(()=>Scan(trip,driver.Id,true));
    await Use(async s=>{
        await s.GetRequiredService<BookingService>().SetTripState(driver.Id,false,trip,TripStatus.Boarding,null);
        return 0;
    });
    try{await Scan(trip,otherDriver.Id,true);throw new Exception("Unassigned driver accepted");}
    catch(BusinessException e)when(e.Status==403){}
    try{await Scan(wrongTrip,driver.Id,true);throw new Exception("Wrong trip accepted");}
    catch(BusinessException e)when(e.Status==404){}
    await Task.WhenAll(Scan(trip,driver.Id,true),Scan(trip,staff.Id,false));
    await Use(async s=>{
        var db=s.GetRequiredService<AppDbContext>();
        Assert(await db.Tickets.AnyAsync(t=>t.Id==ticket.Id&&t.CheckedInUtc!=null),"Check-in was not recorded");
        Assert(await db.AuditEntries.CountAsync(a=>a.Action=="TicketCheckedIn"&&a.EntityId==ticket.Id.ToString())==1,"Duplicate check-in audit");
        return 0;
    });
});
await Run("Snapshot_persistence_new_scope_no_extension",async()=>{var t=await NewTrip();var h=await Hold(actors[0].Id,t,[(await SeatIds(t))[0]]);var snap=await Use(s=>s.GetRequiredService<BookingService>().Snapshot(t,actors[0].Id));Assert(snap.MyHolds.Single().Id==h.Id&&snap.MyHolds.Single().ExpiresAtUtc==h.ExpiresAtUtc,"Scope reload lost/extended hold");await Release(actors[0].Id,h.Id);});
await Run("Final_SQL_deadline_exchange_rollback_after_insert_wait",async()=>{
 var t=await NewTrip();var target=await NewTrip();var oldHold=await Hold(actors[0].Id,t,[(await SeatIds(t))[0]]);var oldId=await Confirm(actors[0].Id,oldHold);
 await Pay(actors[0].Id,oldId);
 await Use(async s=>{await s.GetRequiredService<BookingService>().RequestChange(actors[0].Id,new RequestForm{BookingId=oldId,Kind=ChangeKind.Exchange,Reason="Kiểm rollback tại cập nhật hạn cuối"});return 0;});
 var next=await Hold(staff.Id,target,[(await SeatIds(target))[0]]);var trigger="TestDelay_"+run;
 // Fixture-only failure injection on an isolated _Tests DB. Names/GUID are generated internally, never user input.
 await Use(async s=>await s.GetRequiredService<AppDbContext>().Database.ExecuteSqlRawAsync($"CREATE TRIGGER dbo.[{trigger}] ON dbo.Bookings AFTER INSERT AS BEGIN IF EXISTS(SELECT 1 FROM inserted WHERE SeatHoldId='{next.Id:D}') WAITFOR DELAY '00:00:03'; END"));
 try{
  await Use(async s=>await s.GetRequiredService<AppDbContext>().Database.ExecuteSqlInterpolatedAsync($"UPDATE dbo.SeatHolds SET CreatedUtc=DATEADD(second,-298,SYSUTCDATETIME()), ExpiresAtUtc=DATEADD(second,2,SYSUTCDATETIME()) WHERE Id={next.Id}"));
  var watch=Stopwatch.StartNew();await Conflict(()=>Confirm(staff.Id,next,oldId,true));Assert(watch.Elapsed.TotalMilliseconds>=2900,"Fixture expired before insert trigger; cannot claim late-write rollback evidence.");
  await Use(async s=>{var db=s.GetRequiredService<AppDbContext>();Assert((await db.Bookings.SingleAsync(x=>x.Id==oldId)).Status==BookingStatus.Active,"Late expiry lost old booking");Assert(!await db.Bookings.AnyAsync(x=>x.SeatHoldId==next.Id),"Late expiry committed replacement");return 0;});await DbInvariant(t);await DbInvariant(target);
 }finally{await Use(async s=>await s.GetRequiredService<AppDbContext>().Database.ExecuteSqlRawAsync($"DROP TRIGGER IF EXISTS dbo.[{trigger}]"));}
});
await Run("Vehicle_schedule_and_configuration_guards",async()=>{
 var t=await NewTrip();var existing=await Use(s=>s.GetRequiredService<AppDbContext>().Trips.AsNoTracking().SingleAsync(x=>x.Id==t));
 await Conflict(async()=>{await Use(async s=>{await s.GetRequiredService<CatalogService>().SaveTrip("test",new TripForm{BusRouteId=routeId,VehicleId=vehicleId,DepartureLocal=BusinessTime.Vietnam(existing.DepartureUtc).AddMinutes(30),ArrivalLocal=BusinessTime.Vietnam(existing.ArrivalUtc).AddMinutes(30),Fare=150000});return 0;});});
 await Conflict(async()=>{await Use(async s=>{await s.GetRequiredService<CatalogService>().SaveLayout("test",new LayoutForm{Id=layoutId,Name="Cannot reshape",Rows=5,Columns=4,Floors=1});return 0;});});
 var h=await Hold(actors[0].Id,t,[(await SeatIds(t))[0]]);
 await Conflict(async()=>{await Use(async s=>{await s.GetRequiredService<CatalogService>().SaveTrip("test",new TripForm{Id=t,BusRouteId=routeId,VehicleId=vehicleId,DepartureLocal=BusinessTime.Vietnam(existing.DepartureUtc),ArrivalLocal=BusinessTime.Vietnam(existing.ArrivalUtc),Fare=999999});return 0;});});
 await Release(actors[0].Id,h.Id);
});
await Run("Database_filtered_unique_ticket_constraint",async()=>{
 var t=await NewTrip();var h=await Hold(actors[0].Id,t,[(await SeatIds(t))[0]]);var bookingId=await Confirm(actors[0].Id,h);
 await Use(async s=>{var db=s.GetRequiredService<AppDbContext>();await using var tx=await db.Database.BeginTransactionAsync();
 db.Tickets.Add(new Ticket{Id=Guid.NewGuid(),BookingId=bookingId,TripSeatId=h.SeatIds[0],PassengerName="Duplicate must fail",Fare=150000});
 try{await db.SaveChangesAsync();throw new Exception("Filtered unique index accepted duplicate ticket");}catch(DbUpdateException e)when(e.InnerException is SqlException sql&&sql.Number is 2601 or 2627){}await tx.RollbackAsync();return 0;});
 await DbInvariant(t);
});
if(args.Contains("--five-minutes"))await Run("REAL_300_second_hold_then_expiry",async()=>{var t=await NewTrip();var h=await Hold(actors[0].Id,t,[(await SeatIds(t))[0]]);var sw=Stopwatch.StartNew();Console.WriteLine("Waiting actual 300s + 2s. This is a TEST wait, never a booking lock.");await Task.Delay(TimeSpan.FromSeconds(302));await Use(async s=>{await s.GetRequiredService<BookingService>().ExpireTrip(t);return 0;});await Conflict(()=>Confirm(actors[0].Id,h));await Use(async s=>{var row=await s.GetRequiredService<AppDbContext>().SeatHolds.SingleAsync(x=>x.Id==h.Id);Assert(row.Status==HoldStatus.Expired,"Not expired");return 0;});await File.WriteAllTextAsync(Path.Combine(output,"five-minutes.json"),JsonSerializer.Serialize(new{h.Id,h.ExpiresAtUtc,waitSeconds=sw.Elapsed.TotalSeconds},json));});
else cases.Add(new{name="REAL_300_second_hold_then_expiry",status="CHƯA KIỂM CHỨNG",reason="Chạy với --five-minutes để chờ đủ 300 giây thực."});

// A separate HTTP run supplies independent cookies, valid anti-forgery tokens and a simultaneous release barrier.
var httpIndex=Array.IndexOf(args,"--http");var baseUrl=httpIndex>=0&&httpIndex+1<args.Length?args[httpIndex+1].TrimEnd('/'):null;
async Task<(HttpClient Client,string Csrf)> Login(string email){var handler=new HttpClientHandler{CookieContainer=new CookieContainer(),AllowAutoRedirect=false};var client=new HttpClient(handler){BaseAddress=new Uri(baseUrl!),Timeout=TimeSpan.FromSeconds(60)};var html=await client.GetStringAsync("/Account/Login");var match=Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");Assert(match.Success,"No login anti-forgery token");var response=await client.PostAsync("/Account/Login",new FormUrlEncodedContent(new Dictionary<string,string>{{"Email",email},{"Password",password},{"__RequestVerificationToken",WebUtility.HtmlDecode(match.Groups[1].Value)}}));Assert(response.StatusCode==HttpStatusCode.Redirect,"Login failed: "+(int)response.StatusCode);var csrf=await client.GetFromJsonAsync<JsonElement>("/api/seats/csrf");return (client,csrf.GetProperty("token").GetString()!);}
if(baseUrl!=null) {
 // Abort, rather than award false wins, if the app is connected to a different database.
 var probe=await Login(actors[0].Email);using(probe.Client){var snap=await probe.Client.GetFromJsonAsync<SeatSnapshot>("/api/seats/"+browserTrip,json);Assert(snap!=null&&snap.Seats.Any(x=>x.Id==browserSeats[0]),"HTTP app not using this test DB/fixture.");}
 foreach(var n in new[]{20,50,100})await Run("HTTP_concurrency_"+n,async()=>{
  var t=await NewTrip();var seat=(await SeatIds(t))[0];var sessions=new List<(HttpClient Client,string Csrf)>();
  try {
   var participants=actors.Take(n).ToList();if(n==20)participants[n-1]=staff;
   foreach(var a in participants)sessions.Add(await Login(a.Email));
   var start=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
   var tasks=sessions.Select((session,i)=>Task.Run(async()=>{await start.Task;var watch=Stopwatch.StartNew();var body=new HoldForm{TripId=t,SeatIds=[seat],RequestKey=Guid.NewGuid()};session.Client.DefaultRequestHeaders.Add("X-CSRF-TOKEN",session.Csrf);try{var r=await session.Client.PostAsJsonAsync("/api/seats/hold",body);var text=await r.Content.ReadAsStringAsync();return new Sample(i,(int)r.StatusCode,watch.Elapsed.TotalMilliseconds,text);}catch(Exception e){return new Sample(i,0,watch.Elapsed.TotalMilliseconds,e.ToString());}})).ToArray();
   var startedUtc=DateTime.UtcNow;start.SetResult(true);var samples=await Task.WhenAll(tasks);
   var winners=samples.Count(x=>x.Status==200);var conflicts=samples.Count(x=>x.Status==409);var errors=samples.Length-winners-conflicts;
   var dbState=await Use(async s=>{var db=s.GetRequiredService<AppDbContext>();var now=await s.GetRequiredService<DbGate>().Now();var active=await db.SeatHolds.CountAsync(h=>h.TripId==t&&h.Status==HoldStatus.Active&&h.ExpiresAtUtc>now);var tickets=await db.Tickets.CountAsync(x=>x.TripSeatId==seat&&x.IsActive);var duplicate=await db.Tickets.Where(x=>x.TripSeatId==seat&&x.IsActive).GroupBy(x=>x.TripSeatId).CountAsync(g=>g.Count()>1);return new{activeHolds=active,activeTickets=tickets,doubleBooking=duplicate};});
   var latencies=samples.Select(x=>x.Ms).Order().ToArray();var p95=latencies[(int)Math.Ceiling(.95*latencies.Length)-1];
   var record=new{sourceMix=n==20?"19 Customer + 1 Staff":"Customer",concurrent=n,sent=n,completed=samples.Count(x=>x.Status!=0),newWinners=winners,validConflicts=conflicts,unexpectedErrors=errors,meanMs=latencies.Average(),p95Ms=p95,technicalSuccessRate=(double)(winners+conflicts)/n,dbState,startedUtc,method="Independent cookies + CSRF, barrier, HTTP, nearest-rank P95, monotonic request timer",samples};races.Add(record);
   await File.WriteAllTextAsync(Path.Combine(output,$"concurrency-{n}.json"),JsonSerializer.Serialize(record,json));
   await File.WriteAllLinesAsync(Path.Combine(output,$"concurrency-{n}.csv"),new[]{"client,status,response_ms"}.Concat(samples.Select(x=>$"{x.Index},{x.Status},{x.Ms.ToString("F4",System.Globalization.CultureInfo.InvariantCulture)}")));
   Assert(winners==1&&conflicts==n-1&&errors==0&&dbState.activeHolds==1&&dbState.activeTickets==0&&dbState.doubleBooking==0,"Invalid concurrency outcome. Inspect raw logs; never count auth/CSRF/500 as conflicts.");
   var winner=samples.Single(x=>x.Status==200);var held=JsonSerializer.Deserialize<HoldView>(winner.Body,json)!;await Release(participants[winner.Index].Id,held.Id);
  }finally{foreach(var x in sessions)x.Client.Dispose();}
 });
} else foreach(var n in new[]{20,50,100})cases.Add(new{name="HTTP_concurrency_"+n,status="CHƯA KIỂM CHỨNG",reason="Cần --http URL và app chạy trên cùng database _Tests."});
await File.WriteAllTextAsync(Path.Combine(output,"results.json"),JsonSerializer.Serialize(new{run,utc=DateTime.UtcNow,dotnet=Environment.Version.ToString(),os=Environment.OSVersion.ToString(),processors=Environment.ProcessorCount,database=cs.InitialCatalog,sqlVersion=await Use(async s=>await s.GetRequiredService<AppDbContext>().Database.SqlQueryRaw<string>("SELECT CAST(@@VERSION AS nvarchar(max)) AS [Value]").SingleAsync()),cases,races},json));
Console.WriteLine("Raw results: "+output);Console.WriteLine("Fixtures remain as isolated historical SQL test data. No user database was reset/deleted.");
record Sample(int Index,int Status,double Ms,string Body);
