using System.ComponentModel.DataAnnotations;
using BusBooking.Models;
namespace BusBooking.ViewModels;
public class LoginForm {
    [Required,Display(Name="Tên đăng nhập hoặc email")] public virtual string Email {get;set;}="";
    [Required,DataType(DataType.Password),Display(Name="Mật khẩu")] public string Password {get;set;}="";
    public string? ReturnUrl {get;set;}
}
public class RegisterForm {
    [Required,EmailAddress,Display(Name="Email")] public string Email {get;set;}="";
    [Required,StringLength(100,MinimumLength=2),Display(Name="Họ tên")] public string FullName {get;set;}="";
    [Required(ErrorMessage="Vui lòng nhập mật khẩu."),MinLength(AppUser.MinimumPasswordLength,ErrorMessage="Mật khẩu cần ít nhất {1} ký tự."),DataType(DataType.Password),Display(Name="Mật khẩu")]
    public string Password {get;set;}="";
    [Required(ErrorMessage="Vui lòng nhập lại mật khẩu."),Compare(nameof(Password),ErrorMessage="Hai mật khẩu không khớp."),DataType(DataType.Password),Display(Name="Nhập lại mật khẩu")]
    public string ConfirmPassword {get;set;}="";
}
public class ProfileForm {
    [Required,StringLength(100,MinimumLength=2),Display(Name="Họ tên")] public string FullName {get;set;}="";
    [Phone,StringLength(25),Display(Name="Số điện thoại")] public string? Phone {get;set;}
    [DataType(DataType.Date),Display(Name="Ngày sinh")] public DateOnly? DateOfBirth {get;set;}
    [StringLength(300),Display(Name="Địa chỉ")] public string? Address {get;set;}
    [StringLength(2),Display(Name="Tỉnh / thành phố")] public string? ProvinceCode {get;set;}
    public string UserId {get;set;}="";
    public string UserName {get;set;}="";
    public string Email {get;set;}="";
    public string RoleLabel {get;set;}="";
    public bool IsActive {get;set;}
    public DateTime CreatedUtc {get;set;}
}
public class NewPasswordForm {
    [Required(ErrorMessage="Vui lòng nhập mật khẩu mới."),DataType(DataType.Password),Display(Name="Mật khẩu mới")]
    [MinLength(AppUser.MinimumPasswordLength,ErrorMessage="Mật khẩu cần ít nhất {1} ký tự.")]
    public string NewPassword {get;set;}="";
    [Required(ErrorMessage="Vui lòng nhập lại mật khẩu mới."),Compare(nameof(NewPassword),ErrorMessage="Hai mật khẩu mới không khớp."),DataType(DataType.Password),Display(Name="Nhập lại mật khẩu mới")]
    public string ConfirmPassword {get;set;}="";
}
public class ChangePasswordForm : NewPasswordForm {
    [Required(ErrorMessage="Vui lòng nhập mật khẩu hiện tại."),DataType(DataType.Password),Display(Name="Mật khẩu hiện tại")]
    public string CurrentPassword {get;set;}="";
}
public class HoldForm {
    public int? PickupStopId {get;set;} public int? DropoffStopId {get;set;}
    [Range(1,int.MaxValue)] public int TripId {get;set;}
    [Required,MinLength(1),MaxLength(6)] public int[] SeatIds {get;set;}=[];
    public Guid RequestKey {get;set;}
}
public class TokenForm {public Guid HoldId {get;set;}}
public class PassengerForm {
    public int TripSeatId {get;set;}
    [Required,StringLength(100,MinimumLength=2)] public string Name {get;set;}="";
}
public class ConfirmForm:TokenForm {
    public int? PickupStopId {get;set;} public int? DropoffStopId {get;set;}
    [Required,StringLength(100,MinimumLength=2)] public string ContactName {get;set;}="";
    [Required,RegularExpression(@"^\+?[0-9 ()\-]{8,25}$")] public string Phone {get;set;}="";
    [Required,MinLength(1),MaxLength(6)] public List<PassengerForm> Passengers {get;set;}=[];
    [EmailAddress,StringLength(256)] public string? CustomerEmail {get;set;}
    public Guid? ExchangeBookingId {get;set;}
}
public class RouteForm {
    public int Id {get;set;}
    [Required,StringLength(100),Display(Name="Điểm đi")] public string Origin {get;set;}="";
    [Required,StringLength(100),Display(Name="Điểm đến")] public string Destination {get;set;}="";
    [Range(1,2880),Display(Name="Thời gian dự kiến (phút)")] public int DurationMinutes {get;set;}=180;
    [StringLength(500),Display(Name="Mô tả")] public string Description {get;set;}="";
    [Display(Name="Hoạt động")] public bool IsActive {get;set;}=true;
}
public class LayoutForm {
    public int Id {get;set;}
    [Required,StringLength(100),Display(Name="Tên sơ đồ")] public string Name {get;set;}="";
    [Range(1,20),Display(Name="Số hàng")] public int Rows {get;set;}=8;
    [Range(1,4),Display(Name="Ghế mỗi hàng")] public int Columns {get;set;}=4;
    [Range(1,2),Display(Name="Số tầng")] public int Floors {get;set;}=1;
    [Display(Name="Hoạt động")] public bool IsActive {get;set;}=true;
}
public class VehicleForm {
    public int Id {get;set;}
    [Required,StringLength(30),Display(Name="Biển số")] public string Plate {get;set;}="";
    [Required,StringLength(100),Display(Name="Tên xe")] public string Name {get;set;}="";
    [Required(ErrorMessage="Vui lòng chọn loại xe."),EnumDataType(typeof(VehicleKind)),Display(Name="Loại xe")]
    public VehicleKind? Kind {get;set;}
    [Range(1,int.MaxValue),Display(Name="Sơ đồ ghế")] public int SeatLayoutId {get;set;}
    [Display(Name="Trạng thái")] public bool IsActive {get;set;}=true;
}
public class DriverForm {
    public string? Id {get;set;}
    [Required,StringLength(100,MinimumLength=2),Display(Name="Họ và tên")] public string FullName {get;set;}="";
    [Required,EmailAddress,StringLength(256),Display(Name="Email")] public string Email {get;set;}="";
    [Phone,StringLength(25),Display(Name="Số điện thoại")] public string? PhoneNumber {get;set;}
    [DataType(DataType.Date),Display(Name="Ngày sinh")] public DateOnly? DateOfBirth {get;set;}
    [MinLength(AppUser.MinimumPasswordLength,ErrorMessage="Mật khẩu cần ít nhất {1} ký tự."),DataType(DataType.Password),Display(Name="Mật khẩu ban đầu")] public string? Password {get;set;}
    [Display(Name="Trạng thái")] public bool IsActive {get;set;}=true;
}
public class TripForm {
    public int Id {get;set;}
    [Range(1,int.MaxValue),Display(Name="Tuyến")] public int BusRouteId {get;set;}
    [Range(1,int.MaxValue),Display(Name="Xe")] public int VehicleId {get;set;}
    [Display(Name="Khởi hành — giờ Việt Nam"),DataType(DataType.DateTime)] public DateTime DepartureLocal {get;set;}=DateTime.UtcNow.AddDays(1).AddHours(7);
    [Display(Name="Đến dự kiến — giờ Việt Nam"),DataType(DataType.DateTime)] public DateTime ArrivalLocal {get;set;}=DateTime.UtcNow.AddDays(1).AddHours(12);
    [Range(typeof(decimal),"0","10000000"),Display(Name="Giá mỗi ghế (VND)")] public decimal Fare {get;set;}=200000;
    [Display(Name="Tài xế chính")] public string? DriverId {get;set;}
    [Display(Name="Tài xế thứ hai")] public string? CoDriverId {get;set;}
    [Display(Name="Nhân viên phụ trách")] public string? StaffId {get;set;}
}
public class RoleForm { [Required] public string UserId {get;set;}="";[Required] public string Role {get;set;}="Customer"; public bool IsActive {get;set;}=true; }
public class RequestForm {
    public Guid BookingId {get;set;}
    [EnumDataType(typeof(ChangeKind))] public ChangeKind Kind {get;set;}
    [Required,StringLength(500,MinimumLength=5)] public string Reason {get;set;}="";
}
public class PaymentForm {
    public Guid BookingId {get;set;}
    public Guid RequestKey {get;set;}
    public bool Refund {get;set;}
    public long? Amount {get;set;}
    [StringLength(300)] public string? Note {get;set;}
}
public class SeatView {
    public int Id {get;set;} public string Code {get;set;}="";
    public int Row {get;set;}public int Column {get;set;}public int Floor {get;set;}
    public string Status {get;set;}="";public bool Mine {get;set;}
}
public record HoldView(Guid Id,string Status,DateTime ExpiresAtUtc,int[] SeatIds,int? PickupStopId=null,int? DropoffStopId=null);
public record SeatSnapshot(int TripId,long Revision,DateTime ServerUtc,bool CanBook,decimal Fare,int Columns,List<SeatView> Seats,List<HoldView> MyHolds,List<StopView> Stops,int ArriveEarlyMinutes);
public record TripEvent(int TripId,long Revision,DateTime CommittedObservedUtc);
public class DashboardVm {
    public DateTime From {get;set;} public DateTime To {get;set;}
    public int Trips {get;set;}public int Tickets {get;set;}public int Active {get;set;}public int Cancelled {get;set;}
    public int Paid {get;set;}public int Unpaid {get;set;}
    public int ClosedTickets=>Tickets-Paid-Unpaid;
    public int CompletedTrips {get;set;}
    public int RunningTrips {get;set;}
    public int ScheduledTrips {get;set;}
    public int CancelledTrips {get;set;}
    public int OverdueTrips=>Trips-CompletedTrips-RunningTrips-ScheduledTrips-CancelledTrips;
    public decimal Receipts {get;set;}public decimal Refunds {get;set;}public decimal RefundDue {get;set;}
    public List<RevenueRow> Revenue {get;set;}=[];
    public List<RevenueTrendPoint> RevenueTrend {get;set;}=[];
    public decimal PreviousReceipts {get;set;}
    public int ActiveVehicles {get;set;}
    public int ActiveRoutes {get;set;}
    public int DeparturesToday {get;set;}
    public List<UpcomingDeparture> Upcoming {get;set;}=[];
}
public record RevenueRow(string Label,decimal Receipts,decimal Refunds);
public record RevenueTrendPoint(string Label,decimal Receipts,decimal PreviousReceipts);
public record UpcomingDeparture(int Id,string Origin,string Destination,string Plate,DateTime DepartureUtc,TripStatus Status);
