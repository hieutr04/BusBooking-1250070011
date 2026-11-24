using System.ComponentModel.DataAnnotations;
namespace BusBooking.ViewModels;

public sealed class StopAppendForm {
    [Range(1,int.MaxValue)] public int TripId {get;set;}
    [Range(0,long.MaxValue)] public long Revision {get;set;}
    [Required(ErrorMessage="Vui lòng nhập tên điểm."),StringLength(160,ErrorMessage="Tên điểm tối đa 160 ký tự.")] public string Name {get;set;}="";
    [StringLength(500,ErrorMessage="Địa chỉ tối đa 500 ký tự.")] public string? Address {get;set;}
    [Range(1,10079,ErrorMessage="Thời gian điểm phải nằm giữa giờ đi và giờ đến.")] public int OffsetMinutes {get;set;}
    public bool CanPickup {get;set;}=true;
    public bool CanDropoff {get;set;}=true;
}
