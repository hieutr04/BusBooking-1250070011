using BusBooking.Models;
namespace BusBooking.Services;

// Read-only labels for known operation metadata. The stored audit record remains untouched.
public static class AuditPresentation {
 public static string Actor(string actor)=>actor switch {"worker"=>"Xử lý tự động","command"=>"Lệnh quản trị","seed"=>"Khởi tạo",_=>actor};
 public static string Action(string action)=>action switch {
  "RouteSaved"=>"Lưu tuyến","LayoutSaved"=>"Lưu sơ đồ ghế","VehicleSaved"=>"Lưu xe","TripSaved"=>"Lưu chuyến",
  "SpecificStopReviewed"=>"Duyệt điểm cụ thể","SpecificStopSource"=>"Căn cứ duyệt điểm","TripStopsSaved"=>"Lưu lịch điểm","TripStopsUpdated"=>"Bổ sung lịch điểm","TripStopsAppended"=>"Thêm điểm đón/trả","TripStopDefaultsAdded"=>"Tạo lịch điểm",
  "UserRoleChanged"=>"Cập nhật quyền tài khoản","HoldAcquired"=>"Giữ ghế","HoldExpired"=>"Hết hạn giữ ghế","HoldReleased"=>"Nhả ghế",
  "DriverPasswordSet"=>"Đặt mật khẩu tài xế","DriverPasswordReset"=>"Cấp lại mật khẩu tài xế",
  "PaymentSimulated"=>"Ghi nhận thanh toán mô phỏng",
  "QrPaymentCompleted"=>"Hoàn tất thanh toán QR",
  "ScheduleExtended"=>"Bổ sung lịch chuyến",
  "UserProfileInitialized"=>"Bổ sung hồ sơ và thông tin đăng nhập",
  "BookingConfirmed"=>"Xác nhận vé","BookingExchanged"=>"Đổi vé","BookingCancelled"=>"Hủy vé","ReceiptRecorded"=>"Ghi nhận thu","RefundRecorded"=>"Ghi nhận hoàn",
  "ChangeRequested"=>"Yêu cầu đổi/hủy","TripStateChanged"=>"Cập nhật trạng thái chuyến",_=>action};
 public static string? Detail(AuditEntry row) {
  if(row.Action=="SpecificStopReviewed")return "Đã duyệt địa điểm và quyền đón/trả; giờ theo lịch chuyến đang cấu hình.";
  if(row.Action=="TripStopDefaultsAdded"&&row.Detail=="source=dalat-420-v1; illustrative=true")return "Đã thêm các điểm đón/trả theo cấu hình của chiều tuyến.";
  if(row.Action=="TripStopsAppended"&&row.Detail?.StartsWith("insert-only; source=configured-stops; count=",StringComparison.Ordinal)==true) {
   var count=row.Detail[(row.Detail.LastIndexOf('=')+1)..];
   if(int.TryParse(count,out var number))return $"Thêm {number} điểm; giữ nguyên lịch và thông tin đặt vé hiện có.";
  }
  if(row.Action=="TripStopsUpdated")return row.Detail switch {"endpoints-added"=>"Đã thêm điểm đầu và điểm cuối.","study-stops-added"=>"Đã thêm điểm theo cấu hình chiều tuyến.",_=>row.Detail};
  return row.Detail;
 }
}
