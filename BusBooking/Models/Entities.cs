using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace BusBooking.Models;
public static class Roles {
    public const string Admin="Admin", Staff="Staff", Driver="Driver", Customer="Customer";
    public static readonly string[] All=[Admin,Staff,Driver,Customer];
    public static string Label(string role)=>role switch {
        Admin=>"Quản trị viên",Staff=>"Nhân viên",Driver=>"Tài xế",Customer=>"Khách hàng",_=>role
    };
}
public enum SeatStatus { Available, Held, Booked }
public enum HoldStatus { Active, Released, Expired, Confirmed }
public enum TripStatus { Scheduled, Boarding, InProgress, Completed, Cancelled }
public enum BookingStatus { Active, Cancelled, Exchanged, Expired }
public enum RequestStatus { Pending, Approved, Rejected }
public enum ChangeKind { Cancel, Exchange }
public enum MoneyKind { Receipt, Refund }
public enum VehicleKind {
    [Display(Name="Xe ghế ngồi")] Seated,
    [Display(Name="Xe giường nằm")] Sleeper
}
public class AppUser:IdentityUser {
    public const int MinimumPasswordLength=6;
    [MaxLength(100)] public string FullName {get;set;}="";
    public DateOnly? DateOfBirth {get;set;}
    [MaxLength(300)] public string? Address {get;set;}
    [MaxLength(2)] public string? ProvinceCode {get;set;}
    public DateTime CreatedUtc {get;set;}
    public bool IsActive {get;set;}=true;
}
public class BusRoute {
    public int Id {get;set;}
    [MaxLength(100)] public string Origin {get;set;}="";
    [MaxLength(100)] public string Destination {get;set;}="";
    [MaxLength(500)] public string Description {get;set;}="";
    public int DurationMinutes {get;set;}
    public bool IsActive {get;set;}=true;
    public string Label=>$"{Origin} → {Destination}";
}
public class SeatLayout {
    public int Id {get;set;}
    [MaxLength(100)] public string Name {get;set;}="";
    public int Rows {get;set;} public int Columns {get;set;} public int Floors {get;set;}=1;
    public bool IsActive {get;set;}=true;
    public List<PhysicalSeat> Seats {get;set;}=[];
}
public class PhysicalSeat {
    public int Id {get;set;} public int SeatLayoutId {get;set;}
    public SeatLayout Layout {get;set;}=null!;
    [MaxLength(10)] public string Code {get;set;}="";
    public int Row {get;set;} public int Column {get;set;} public int Floor {get;set;}=1;
}
public class Vehicle {
    public int Id {get;set;}
    [MaxLength(30)] public string Plate {get;set;}="";
    [MaxLength(100)] public string Name {get;set;}="";
    public VehicleKind Kind {get;set;}
    public int SeatLayoutId {get;set;} public SeatLayout Layout {get;set;}=null!;
    public bool IsActive {get;set;}=true;
}
public class Trip {
    public int Id {get;set;} public int BusRouteId {get;set;} public BusRoute Route {get;set;}=null!;
    public int VehicleId {get;set;} public Vehicle Vehicle {get;set;}=null!;
    public DateTime DepartureUtc {get;set;} public DateTime ArrivalUtc {get;set;}
    public decimal Fare {get;set;}
    public TripStatus Status {get;set;}=TripStatus.Scheduled;
    public bool SalesOpen {get;set;}=true;
    public long Revision {get;set;}
    public string? DriverId {get;set;} public AppUser? Driver {get;set;}
    public string? CoDriverId {get;set;} public AppUser? CoDriver {get;set;}
    public string? StaffId {get;set;} public AppUser? Staff {get;set;}
    public List<TripSeat> Seats {get;set;}=[];
}
public class TripSeat {
    public int Id {get;set;} public int TripId {get;set;} public Trip Trip {get;set;}=null!;
    public int PhysicalSeatId {get;set;} public PhysicalSeat PhysicalSeat {get;set;}=null!;
    // Immutable seat geometry snapshot on this departure.
    [MaxLength(10)] public string Code {get;set;}="";
    public int Row {get;set;} public int Column {get;set;} public int Floor {get;set;}
    public SeatStatus Status {get;set;}
    public Guid? CurrentHoldId {get;set;} public SeatHold? CurrentHold {get;set;}
}
public class SeatHold {
    public int? PickupStopId {get;set;} public int? DropoffStopId {get;set;}
    public Guid Id {get;set;} public int TripId {get;set;} public Trip Trip {get;set;}=null!;
    [MaxLength(450)] public string OwnerId {get;set;}=""; public AppUser Owner {get;set;}=null!;
    public Guid RequestKey {get;set;}
    public DateTime CreatedUtc {get;set;} public DateTime ExpiresAtUtc {get;set;}
    public HoldStatus Status {get;set;}
    public List<HoldSeat> Items {get;set;}=[];
}
public class HoldSeat {
    public Guid SeatHoldId {get;set;} public SeatHold Hold {get;set;}=null!;
    public int TripSeatId {get;set;} public int TripId {get;set;} public TripSeat Seat {get;set;}=null!;
}
public class Booking {
    public int? PickupStopId {get;set;} public int? DropoffStopId {get;set;}
    [MaxLength(160)] public string? PickupName {get;set;}
    [MaxLength(160)] public string? DropoffName {get;set;}
    [MaxLength(500)] public string? PickupAddress {get;set;}
    [MaxLength(500)] public string? DropoffAddress {get;set;}
    public DateTime? PickupAtUtc {get;set;} public DateTime? DropoffAtUtc {get;set;}
    public bool? StopsIllustrative {get;set;}

    public Guid Id {get;set;} public Guid SeatHoldId {get;set;} public SeatHold Hold {get;set;}=null!;
    [MaxLength(30)] public string Code {get;set;}="";
    [MaxLength(450)] public string OwnerId {get;set;}=""; public AppUser Owner {get;set;}=null!;
    [MaxLength(100)] public string ContactName {get;set;}="";
    [MaxLength(25)] public string ContactPhone {get;set;}="";
    public DateTime CreatedUtc {get;set;} public BookingStatus Status {get;set;}
    public decimal Total {get;set;}
    public DateTime? SimulatedPaidUtc {get;set;}
    public DateTime? PaymentExpiresUtc {get;set;}
    public decimal? SimulatedPaidAmount {get;set;}
    public Guid? ReplacesBookingId {get;set;}
    public List<Ticket> Tickets {get;set;}=[];
    public List<PaymentEntry> Payments {get;set;}=[];
    public decimal Balance=>Payments.Sum(p=>p.Amount);
}
public class Ticket {
    public DateTime? CheckedInUtc {get;set;}
    public Guid Id {get;set;} public Guid BookingId {get;set;} public Booking Booking {get;set;}=null!;
    // Trip is derived through TripSeat; no conflicting TripId can be posted.
    public int TripSeatId {get;set;} public TripSeat Seat {get;set;}=null!;
    [MaxLength(100)] public string PassengerName {get;set;}="";
    public decimal Fare {get;set;} public bool IsActive {get;set;}=true;
}
public class PaymentEntry {
    public Guid Id {get;set;} public Guid BookingId {get;set;} public Booking Booking {get;set;}=null!;
    public MoneyKind Kind {get;set;} public decimal Amount {get;set;}
    [MaxLength(450)] public string ActorId {get;set;}="";
    public Guid RequestKey {get;set;} public DateTime CreatedUtc {get;set;}
    [MaxLength(300)] public string Note {get;set;}="";
}
public class ChangeRequest {
    public Guid Id {get;set;} public Guid BookingId {get;set;} public Booking Booking {get;set;}=null!;
    public ChangeKind Kind {get;set;} public RequestStatus Status {get;set;}
    [MaxLength(500)] public string Reason {get;set;}="";
    [MaxLength(500)] public string Resolution {get;set;}="";
    public DateTime CreatedUtc {get;set;} public DateTime? ResolvedUtc {get;set;}
    [MaxLength(450)] public string? ResolvedBy {get;set;}
}
public class AuditEntry {
    public long Id {get;set;} public DateTime CreatedUtc {get;set;}
    [MaxLength(450)] public string ActorId {get;set;}="";
    [MaxLength(60)] public string Action {get;set;}="";
    [MaxLength(100)] public string EntityId {get;set;}="";
    [MaxLength(500)] public string Detail {get;set;}="";
}
