using System.ComponentModel.DataAnnotations;
namespace BusBooking.ViewModels;

public sealed class HoldStopsForm : TokenForm {
    [Range(1,int.MaxValue)] public int PickupStopId { get; set; }
    [Range(1,int.MaxValue)] public int DropoffStopId { get; set; }
}
public sealed class StopEditRow {
    public int Id { get; set; }
    [Required,StringLength(160)] public string Name { get; set; } = "";
    [StringLength(500)] public string? Address { get; set; }
    [Range(0,10080)] public int OffsetMinutes { get; set; }
    public bool CanPickup { get; set; }
    public bool CanDropoff { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsIllustrative { get; set; } = true;
}
public sealed class TripStopsForm {
    public int TripId { get; set; }
    public long Revision { get; set; }
    public string RouteLabel { get; set; } = "";
    public DateTime DepartureUtc { get; set; }
    public DateTime ArrivalUtc { get; set; }
    public bool Locked { get; set; }
    [MinLength(2),MaxLength(20)] public List<StopEditRow> Stops { get; set; } = [];
}
public record StopView(int Id,int StopOrder,string Name,string? Address,DateTime AtUtc,
    bool CanPickup,bool CanDropoff,bool IsIllustrative,bool CanSelect=true,bool LocationSpecific=true);
