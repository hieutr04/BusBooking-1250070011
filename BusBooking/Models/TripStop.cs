using System.ComponentModel.DataAnnotations;
namespace BusBooking.Models;

// Offsets belong to one departure, never change the departure itself.
public sealed class TripStop {
    public int Id { get; set; }
    public int TripId { get; set; }
    public int StopOrder { get; set; }
    [MaxLength(160)] public string Name { get; set; } = "";
    [MaxLength(500)] public string? Address { get; set; }
    public int OffsetMinutes { get; set; }
    public bool CanPickup { get; set; }
    public bool CanDropoff { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsIllustrative { get; set; } = true;
}
