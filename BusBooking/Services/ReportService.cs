using BusBooking.Data;
using BusBooking.Models;
using BusBooking.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace BusBooking.Services;
public class ReportService(AppDbContext db) {
    public async Task<DashboardVm> Dashboard(DateTime? from,DateTime? to,CancellationToken ct=default,string? overviewPeriod=null) {
        var now=DateTime.UtcNow;
        var today=BusinessTime.Vietnam(now).Date;var a=(from??today.AddDays(-29)).Date;var b=(to??today).Date;
        Guard.Input(a.Year>=1900&&b.Year<=9998&&b>=a&&(b-a).TotalDays<=366,"Chọn khoảng thời gian hợp lệ, tối đa 366 ngày.");
        var start=BusinessTime.FromVietnam(a);var end=BusinessTime.FromVietnam(b.AddDays(1));
        var bookings=await db.Bookings.AsNoTracking().Include(x=>x.Tickets).Include(x=>x.Payments).Where(x=>x.CreatedUtc>=start&&x.CreatedUtc<end).ToListAsync(ct);
        var all=await db.Payments.AsNoTracking().Where(x=>x.CreatedUtc>=start&&x.CreatedUtc<end).Select(x=>new{x.Amount,x.CreatedUtc,x.Booking.Hold.Trip.Route.Origin,x.Booking.Hold.Trip.Route.Destination}).ToListAsync(ct);
        var due=await db.Bookings.AsNoTracking().Where(x=>x.Status!=BookingStatus.Active).Select(x=>x.Payments.Sum(p=>(decimal?)p.Amount)??0).ToListAsync(ct);
        var report=new DashboardVm{From=a,To=b,Trips=await db.Trips.CountAsync(x=>x.DepartureUtc>=start&&x.DepartureUtc<end,ct),Tickets=bookings.Sum(x=>x.Tickets.Count),Active=bookings.Sum(x=>x.Tickets.Count(t=>t.IsActive)),Cancelled=bookings.Sum(x=>x.Tickets.Count(t=>!t.IsActive)),Paid=bookings.Where(x=>x.Status==BookingStatus.Active&&x.Balance>=x.Total).Sum(x=>x.Tickets.Count),Unpaid=bookings.Where(x=>x.Status==BookingStatus.Active&&x.Balance<x.Total).Sum(x=>x.Tickets.Count),Receipts=all.Where(x=>x.Amount>0).Sum(x=>x.Amount),Refunds=-all.Where(x=>x.Amount<0).Sum(x=>x.Amount),RefundDue=due.Where(x=>x>0).Sum(),Revenue=all.GroupBy(x=>x.Origin+" → "+x.Destination).Select(g=>new RevenueRow(g.Key,g.Where(x=>x.Amount>0).Sum(x=>x.Amount),-g.Where(x=>x.Amount<0).Sum(x=>x.Amount))).ToList()};
        if(overviewPeriod is null)return report;

        var openBookings=bookings.Where(x=>x.Status==BookingStatus.Active&&
            (x.PaymentExpiresUtc==null||x.PaymentExpiresUtc>now)).ToList();
        report.Paid=openBookings.Where(x=>x.Balance>=x.Total).Sum(x=>x.Tickets.Count(t=>t.IsActive));
        report.Unpaid=openBookings.Where(x=>x.Balance<x.Total).Sum(x=>x.Tickets.Count(t=>t.IsActive));
        var tripCounts=await db.Trips.AsNoTracking()
            .Where(x=>x.DepartureUtc>=start&&x.DepartureUtc<end)
            .GroupBy(x=>new {x.Status,Overdue=x.DepartureUtc<now})
            .Select(g=>new {g.Key.Status,g.Key.Overdue,Count=g.Count()}).ToListAsync(ct);
        report.Trips=tripCounts.Sum(x=>x.Count);
        report.CompletedTrips=tripCounts.Where(x=>x.Status==TripStatus.Completed).Sum(x=>x.Count);
        report.RunningTrips=tripCounts.Where(x=>x.Status==TripStatus.InProgress).Sum(x=>x.Count);
        report.CancelledTrips=tripCounts.Where(x=>x.Status==TripStatus.Cancelled).Sum(x=>x.Count);
        report.ScheduledTrips=tripCounts.Where(x=>(x.Status is TripStatus.Scheduled or TripStatus.Boarding)&&!x.Overdue).Sum(x=>x.Count);

        var days=(b-a).Days+1;
        var byMonth=overviewPeriod=="year";
        var previousDay=byMonth?a.AddYears(-1):a.AddDays(-days);
        var previousStart=BusinessTime.FromVietnam(previousDay);
        var previous=await db.Payments.AsNoTracking()
            .Where(x=>x.Amount>0&&x.CreatedUtc>=previousStart&&x.CreatedUtc<start)
            .Select(x=>new{x.CreatedUtc,x.Amount}).ToListAsync(ct);
        report.PreviousReceipts=previous.Sum(x=>x.Amount);
        if(byMonth) {
            var currentByMonth=all.Where(x=>x.Amount>0).ToLookup(x=>BusinessTime.Vietnam(x.CreatedUtc).Month,x=>x.Amount);
            var previousByMonth=previous.ToLookup(x=>BusinessTime.Vietnam(x.CreatedUtc).Month,x=>x.Amount);
            for(var month=1;month<=12;month++)
                report.RevenueTrend.Add(new($"Th {month}",currentByMonth[month].Sum(),previousByMonth[month].Sum()));
        }else{
            var currentByDay=all.Where(x=>x.Amount>0).ToLookup(x=>(BusinessTime.Vietnam(x.CreatedUtc).Date-a).Days,x=>x.Amount);
            var previousByDay=previous.ToLookup(x=>(BusinessTime.Vietnam(x.CreatedUtc).Date-previousDay).Days,x=>x.Amount);
            var bucketSize=days<=31?1:(days+23)/24;
            for(var offset=0;offset<days;offset+=bucketSize) {
                var length=Math.Min(bucketSize,days-offset);
                var offsets=Enumerable.Range(offset,length);
                var label=a.AddDays(offset).ToString("dd/MM");
                if(length>1)label+="–"+a.AddDays(offset+length-1).ToString("dd/MM");
                report.RevenueTrend.Add(new(label,offsets.Sum(i=>currentByDay[i].Sum()),offsets.Sum(i=>previousByDay[i].Sum())));
            }
        }

        report.ActiveVehicles=await db.Vehicles.CountAsync(x=>x.IsActive,ct);
        report.ActiveRoutes=await db.Routes.CountAsync(x=>x.IsActive,ct);
        var todayStart=BusinessTime.FromVietnam(today);
        var tomorrowStart=todayStart.AddDays(1);
        report.DeparturesToday=await db.Trips.CountAsync(x=>x.DepartureUtc>=todayStart&&x.DepartureUtc<tomorrowStart&&x.Status!=TripStatus.Cancelled,ct);
        report.Upcoming=await db.Trips.AsNoTracking()
            .Where(x=>x.DepartureUtc>=now&&(x.Status==TripStatus.Scheduled||x.Status==TripStatus.Boarding))
            .OrderBy(x=>x.DepartureUtc).ThenBy(x=>x.Id).Take(5)
            .Select(x=>new UpcomingDeparture(x.Id,x.Route.Origin,x.Route.Destination,x.Vehicle.Plate,x.DepartureUtc,x.Status))
            .ToListAsync(ct);
        return report;
    }
}
