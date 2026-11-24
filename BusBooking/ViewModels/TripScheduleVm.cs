using BusBooking.Models;

namespace BusBooking.ViewModels;

public sealed record ScheduleOption(string Value, string Label);

public sealed record TripScheduleVm(
    DateTime Date, int? RouteId, int? VehicleId, string? DriverId,
    int Page, int PageCount, int Total, IReadOnlyList<Trip> Trips,
    IReadOnlyList<ScheduleOption> Routes, IReadOnlyList<ScheduleOption> Vehicles,
    IReadOnlyList<ScheduleOption> Drivers);
