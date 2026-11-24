using System.ComponentModel.DataAnnotations;
namespace BusBooking.ViewModels;
public sealed class SpecificStopForm {
 [Range(1,int.MaxValue)] public int TripId {get;set;}
 [Range(typeof(long),"0","9223372036854775807")] public long Revision {get;set;}
 [Required(ErrorMessage="Nhập tên bến, văn phòng hoặc điểm hẹn cụ thể."),StringLength(160)] public string Name {get;set;}="";
 [Required(ErrorMessage="Nhập địa chỉ đã đối chiếu."),StringLength(500,MinimumLength=5)] public string Address {get;set;}="";
 [Required(ErrorMessage="Nhập khu vực của điểm."),StringLength(160)] public string Area {get;set;}="";
 [Range(0,10080)] public int OffsetMinutes {get;set;}
 public bool CanPickup {get;set;}
 public bool CanDropoff {get;set;}
 [Required(ErrorMessage="Ghi nguồn địa điểm và căn cứ khai thác của chuyến."),StringLength(500)] public string SourceReference {get;set;}="";
 public bool OperatorConfirmed {get;set;}
}
