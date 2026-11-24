using System.ComponentModel.DataAnnotations;
using BusBooking.Services;

namespace BusBooking.ViewModels;

public sealed class TripBatchForm {
    [Range(1,int.MaxValue),Display(Name="Tuyến xe")]
    public int BusRouteId {get;set;}
    [Range(1,int.MaxValue),Display(Name="Xe")]
    public int VehicleId {get;set;}
    [DataType(DataType.Date),Display(Name="Từ ngày")]
    public DateTime From {get;set;}=BusinessTime.Vietnam(DateTime.UtcNow).Date.AddDays(1);
    [DataType(DataType.Date),Display(Name="Đến ngày")]
    public DateTime To {get;set;}=BusinessTime.Vietnam(DateTime.UtcNow).Date.AddDays(7);
    [Required,DataType(DataType.Time),Display(Name="Giờ khởi hành mỗi ngày")]
    public TimeOnly? DepartureTime {get;set;}=new TimeOnly(7,0);
    [Range(0,10000000),Display(Name="Giá mỗi ghế (VND)")]
    public int Fare {get;set;}
    [Required,Display(Name="Tài xế chính")]
    public string DriverId {get;set;}="";
    [Required,Display(Name="Tài xế phụ")]
    public string CoDriverId {get;set;}="";
    [Display(Name="Nhân viên phụ trách")]
    public string? StaffId {get;set;}
}

public sealed record TripBatchRow(DateTime Departure,DateTime Arrival,string Status,int? TripId=null) {
    public bool Ready=>Status=="Có thể tạo";
}
