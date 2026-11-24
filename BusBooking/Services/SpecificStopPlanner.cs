using BusBooking.Models;
namespace BusBooking.Services;

public sealed record SpecificStopRequest(string Name,string Address,int OffsetMinutes,bool CanPickup,bool CanDropoff,
 string Area,string SourceReference,bool OperatorConfirmed);

// A reviewed point is an alternative for new bookings. Existing rows/IDs/flags/times remain intact.
public static class SpecificStopPlanner {
 public static StopAppendPlan Check(Trip trip,IReadOnlyList<TripStop> existing,DateTime now) {
  StopAppendPlan Block(string message)=>new("blocked",message,[]);
  if(trip.Status!=TripStatus.Scheduled||trip.DepartureUtc<=now)return Block("Chỉ bổ sung điểm cho chuyến chưa đến giờ khởi hành.");
  if(existing.Count<2||existing.Any(x=>x.TripId!=trip.Id)||existing.Select(x=>x.StopOrder).Distinct().Count()!=existing.Count)return Block("Lịch hiện tại chưa hợp lệ. Kiểm tra điểm đầu/cuối trước khi bổ sung.");
  var duration=TripStopService.Duration(trip);var ordered=existing.OrderBy(x=>x.StopOrder).ToArray();
  if(ordered[0].OffsetMinutes!=0||ordered[^1].OffsetMinutes!=duration||!ordered[0].IsActive||!ordered[^1].IsActive||!ordered[0].CanPickup||!ordered[^1].CanDropoff)return Block("Lịch hiện tại không khớp điểm đầu/cuối của chuyến.");
  for(int i=1;i<ordered.Length;i++)if(ordered[i].OffsetMinutes<ordered[i-1].OffsetMinutes)return Block("Giờ và thứ tự điểm cũ chưa nhất quán; không tự thay đổi lịch.");
  return new("ready","Có thể duyệt điểm cụ thể cho chuyến này.",[]);
 }
 public static StopAppendPlan Plan(Trip trip,BusRoute route,IReadOnlyList<TripStop> existing,IReadOnlyList<SpecificStopRequest> requested,DateTime now) {
  var check=Check(trip,existing,now);if(check.Blocked)return check;
  StopAppendPlan Block(string message)=>new("blocked",message,[]);
  var duration=TripStopService.Duration(trip);
  if(requested.Count==0||requested.Count>20)return Block("Cần từ 1 đến 20 điểm để duyệt.");
  var working=existing.ToList();var added=new List<TripStop>();
  foreach(var request in requested.OrderBy(x=>x.OffsetMinutes)) {
   var point=new TripStop{TripId=trip.Id,Name=StopPresentation.Name(request.Name),Address=request.Address?.Trim(),OffsetMinutes=request.OffsetMinutes,
    CanPickup=request.CanPickup,CanDropoff=request.CanDropoff,IsActive=true,IsIllustrative=true};
   if(!request.OperatorConfirmed||string.IsNullOrWhiteSpace(request.SourceReference)||request.SourceReference.Trim().Length>500||
    string.IsNullOrWhiteSpace(request.Area)||request.Area.Length>160||point.Name.Length is <3 or >160||point.Address?.Length is not (>=5 and <=500)||
    !StopChoicePolicy.Specific(point,route)||request.OffsetMinutes<0||request.OffsetMinutes>duration||(!request.CanPickup&&!request.CanDropoff))
    return Block("Nhập tên địa điểm cụ thể, địa chỉ, khu vực, căn cứ và xác nhận quyền đón/trả của chuyến; không dùng tên địa phương thay bến/điểm hẹn.");
   if(request.OffsetMinutes==0&&(!request.CanPickup||request.CanDropoff||StopPresentation.Key(request.Area)!=StopPresentation.Key(route.Origin)))
    return Block("Điểm đầu phải thuộc khu vực xuất phát, chỉ mở đón và khớp giờ khởi hành.");
   if(request.OffsetMinutes==duration&&(!request.CanDropoff||request.CanPickup||StopPresentation.Key(request.Area)!=StopPresentation.Key(route.Destination)))
    return Block("Điểm cuối phải thuộc khu vực đến, chỉ mở trả và khớp giờ đến của chuyến.");
   var same=working.Where(x=>StopPresentation.Key(x.Name)==StopPresentation.Key(point.Name)).ToArray();
   if(same.Length>0) {
    if(same.Length!=1||same[0].Address!=point.Address||same[0].OffsetMinutes!=point.OffsetMinutes||same[0].CanPickup!=point.CanPickup||same[0].CanDropoff!=point.CanDropoff||!same[0].IsActive)
     return Block("Điểm "+point.Name+" đã có cấu hình khác. Giữ nguyên để quản trị viên kiểm tra.");
    continue;
   }
   if(working.Count>=20)return Block("Chuyến đã đủ 20 điểm. Không bỏ điểm lịch sử để bổ sung.");
   if(point.OffsetMinutes==0) {
    var min=working.Min(x=>x.StopOrder);if(min==int.MinValue)return Block("Không còn khoảng thứ tự cho điểm đầu mới.");point.StopOrder=min-1;
   } else if(point.OffsetMinutes==duration) {
    var max=working.Max(x=>x.StopOrder);if(max==int.MaxValue)return Block("Không còn khoảng thứ tự cho điểm cuối mới.");point.StopOrder=max+1;
   } else {
    var before=working.Where(x=>x.OffsetMinutes<=point.OffsetMinutes).OrderBy(x=>x.StopOrder).LastOrDefault();
    var after=working.Where(x=>x.OffsetMinutes>point.OffsetMinutes).OrderBy(x=>x.StopOrder).FirstOrDefault();
    if(before==null||after==null||(long)after.StopOrder-before.StopOrder<2)return Block("Không có khoảng thứ tự để chèn; giữ nguyên mọi điểm hiện tại.");
    point.StopOrder=(int)(before.StopOrder+((long)after.StopOrder-before.StopOrder)/2);
   }
   added.Add(point);working.Add(point);
  }
  return added.Count==0?new("complete","Các điểm đã có đúng cấu hình; không thay đổi dữ liệu.",[]):new("ready","Có thể bổ sung điểm cụ thể và giữ nguyên mọi lựa chọn đã phát sinh.",added);
 }
}
