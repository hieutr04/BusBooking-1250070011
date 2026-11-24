using BusBooking.Models;
namespace BusBooking.Services;

// Presentation/selection eligibility only. Never mutates old stops or historic commitments.
public static class StopChoicePolicy {
 private static readonly HashSet<string> Localities=new(StringComparer.Ordinal){"duc trong","bao loc","cam ranh","cam lam","da lat","ho chi minh","nha trang"};
 public static bool Specific(TripStop stop,BusRoute route)=>!string.IsNullOrWhiteSpace(StopPresentation.Address(stop.Address))&&
  (!stop.IsIllustrative||!Localities.Contains(StopPresentation.Key(stop.Name)))&&StopPresentation.Key(stop.Name)!=StopPresentation.Key(route.Origin)&&StopPresentation.Key(stop.Name)!=StopPresentation.Key(route.Destination);
 public static bool LegacyIntermediate(TripStop stop,Trip trip)=>stop.OffsetMinutes>0&&stop.OffsetMinutes<TripStopService.Duration(trip)&&
  (stop.Name.StartsWith("Điểm học tập · ",StringComparison.OrdinalIgnoreCase)||
   stop.Name.EndsWith(" · Mẫu đồ án",StringComparison.OrdinalIgnoreCase)||
   (Localities.Contains(StopPresentation.Key(stop.Name))&&(stop.IsIllustrative||StopPresentation.Address(stop.Address)==null)));
 public static IReadOnlyList<TripStop> ForNew(Trip trip,BusRoute route,IReadOnlyList<TripStop> rows) {
  var eligible=rows.Where(x=>x.TripId==trip.Id&&x.IsActive&&!LegacyIntermediate(x,trip)).ToList();
  var duration=TripStopService.Duration(trip);
  var specificPickup=eligible.Any(x=>x.OffsetMinutes==0&&x.CanPickup&&Specific(x,route));
  var specificDropoff=eligible.Any(x=>x.OffsetMinutes==duration&&x.CanDropoff&&Specific(x,route));
  // Generic endpoints stay as a compatibility fallback until the operator configures a real point.
  // Once a specific replacement exists, only existing holders may still use the old ID.
  return eligible.Where(x=>!(x.OffsetMinutes==0&&specificPickup&&!Specific(x,route))&&
   !(x.OffsetMinutes==duration&&specificDropoff&&!Specific(x,route))).OrderBy(x=>x.StopOrder).ToArray();
 }
 public static (TripStop Pickup,TripStop Dropoff) SelectNew(Trip trip,BusRoute route,IReadOnlyList<TripStop> rows,int? pickup,int? dropoff) {
  var allowed=ForNew(trip,route,rows);var duration=TripStopService.Duration(trip);
  pickup??=allowed.FirstOrDefault(x=>x.OffsetMinutes==0&&x.CanPickup)?.Id;
  dropoff??=allowed.LastOrDefault(x=>x.OffsetMinutes==duration&&x.CanDropoff)?.Id;
  Guard.Input(pickup.HasValue&&dropoff.HasValue,"Lịch điểm đón/trả đang được cập nhật. Vui lòng liên hệ để xác nhận.");
  return TripStopService.Select(trip,allowed,pickup,dropoff);
 }
 public static (TripStop Pickup,TripStop Dropoff) SelectHeld(Trip trip,BusRoute route,IReadOnlyList<TripStop> rows,SeatHold hold,int? pickup,int? dropoff) {
  Guard.Input(hold.TripId==trip.Id,"Giữ chỗ không thuộc chuyến này.");
  pickup??=hold.PickupStopId;dropoff??=hold.DropoffStopId;
  if(!pickup.HasValue||!dropoff.HasValue)return SelectNew(trip,route,rows,pickup,dropoff);
  var allowed=ForNew(trip,route,rows);
  Guard.Input((pickup==hold.PickupStopId||allowed.Any(x=>x.Id==pickup&&x.CanPickup))&&
   (dropoff==hold.DropoffStopId||allowed.Any(x=>x.Id==dropoff&&x.CanDropoff)),"Điểm mới chưa được mở đón/trả cho chuyến này. Lựa chọn đang giữ vẫn được bảo toàn.");
  return TripStopService.Select(trip,rows,pickup,dropoff);
 }
}
