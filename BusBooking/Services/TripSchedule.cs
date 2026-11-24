using BusBooking.Data;
using BusBooking.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Services;

public static class TripSchedule {
    public static async Task<TripScheduleVm> Load(AppDbContext db, DateTime? date,
        int? routeId, int? vehicleId, string? driverId, int page, CancellationToken ct) {
        var day = (date ?? BusinessTime.Vietnam(DateTime.UtcNow)).Date;
        Guard.Input(day > DateTime.MinValue.Date && day < DateTime.MaxValue.Date, "Ngày khởi hành không hợp lệ.");
        var start = BusinessTime.FromVietnam(day);
        var end = start.AddDays(1);
        var query = db.Trips.AsNoTracking().Where(t => t.DepartureUtc >= start && t.DepartureUtc < end);
        if (routeId.HasValue) query = query.Where(t => t.BusRouteId == routeId);
        if (vehicleId.HasValue) query = query.Where(t => t.VehicleId == vehicleId);
        if (!string.IsNullOrWhiteSpace(driverId))
            query = query.Where(t => t.DriverId == driverId || t.CoDriverId == driverId);

        const int pageSize = 25;
        var total = await query.CountAsync(ct);
        var pages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        page = Math.Clamp(page, 1, pages);
        var trips = await query.Include(t => t.Route).Include(t => t.Vehicle)
            .Include(t => t.Driver).Include(t => t.CoDriver).Include(t => t.Staff)
            .OrderBy(t => t.DepartureUtc).ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var routes = await db.Routes.AsNoTracking().OrderBy(r => r.Origin).ThenBy(r => r.Destination).ToListAsync(ct);
        var vehicles = await db.Vehicles.AsNoTracking().OrderBy(v => v.Plate)
            .Select(v => new { v.Id, v.Plate, v.Name }).ToListAsync(ct);
        var drivers = await db.Users.AsNoTracking()
            .Where(u => db.UserRoles.Any(ur => ur.UserId == u.Id && db.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Driver"))
                || db.Trips.Any(t => t.DriverId == u.Id || t.CoDriverId == u.Id))
            .OrderBy(u => u.FullName).ThenBy(u => u.Email)
            .Select(u => new ScheduleOption(u.Id, u.FullName + " · " + u.Email)).ToListAsync(ct);
        return new TripScheduleVm(day, routeId, vehicleId, driverId, page, pages, total, trips,
            routes.Select(r => new ScheduleOption(r.Id.ToString(), r.Label)).ToList(),
            vehicles.Select(v => new ScheduleOption(v.Id.ToString(), v.Plate + " · " + v.Name)).ToList(),
            drivers);
    }
}
