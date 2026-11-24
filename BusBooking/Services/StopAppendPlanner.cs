using BusBooking.Models;
namespace BusBooking.Services;

public sealed record StopAppendRequest(string Name,string? Address,int OffsetMinutes,bool CanPickup=true,bool CanDropoff=true);
public sealed record StopAppendPlan(string Status,string Message,IReadOnlyList<TripStop> NewStops) {
    public bool Ready=>Status=="ready";
    public bool Blocked=>Status=="blocked";
}

// Pure planning: does not mutate any existing entity or read/write a database.
public static class StopAppendPlanner {
    public static bool DalatRoute(BusRoute route)=>StopPresentation.Key(route.Origin)=="da lat"&&StopPresentation.Key(route.Destination)=="ho chi minh";
    public static StopAppendPlan Dalat(Trip trip,BusRoute route,IReadOnlyList<TripStop> existing,DateTime now) {
        if(!DalatRoute(route))return new("not-applicable","Chiều tuyến này chưa có cấu hình bổ sung tự động.",[]);
        if(trip.ArrivalUtc-trip.DepartureUtc!=TimeSpan.FromMinutes(420))
            return new("blocked","Chuyến không có tổng thời gian 7 giờ phù hợp cấu hình hiện có. Vui lòng duyệt giờ từng điểm trong quản lý lịch.",[]);
        // Existing source configuration, not a claimed verified carrier timetable.
        return Plan(trip,route,existing,[new("Đức Trọng",null,90),new("Bảo Lộc",null,180)],now);
    }
    public static StopAppendPlan Check(Trip trip,IReadOnlyList<TripStop> existing,DateTime now) {
        if(trip.Status!=TripStatus.Scheduled||trip.DepartureUtc<=now)
            return new("blocked","Chỉ bổ sung điểm cho chuyến sắp khởi hành, chưa đến giờ đi.",[]);
        if(existing.Any(s=>s.TripId!=trip.Id)||existing.Select(s=>s.StopOrder).Distinct().Count()!=existing.Count)
            return new("blocked","Lịch điểm hiện tại không hợp lệ. Vui lòng kiểm tra trước khi bổ sung.",[]);
        if(existing.Count==0)return new("ready","Có thể tạo lịch điểm cho chuyến này.",[]);
        var ordered=existing.OrderBy(s=>s.StopOrder).ToArray();
        if(ordered[0].StopOrder!=0||ordered[^1].StopOrder!=1000||ordered[0].OffsetMinutes!=0||
           ordered[^1].OffsetMinutes!=TripStopService.Duration(trip)||!ordered[0].IsActive||!ordered[0].CanPickup||
           !ordered[^1].IsActive||!ordered[^1].CanDropoff)
            return new("blocked","Điểm đầu/cuối hiện tại chưa khớp lịch chuyến hoặc chưa mở đón/trả. Giữ nguyên lịch để kiểm tra.",[]);
        for(int i=1;i<ordered.Length;i++)
            if(ordered[i].OffsetMinutes<=ordered[i-1].OffsetMinutes)
                return new("blocked","Thứ tự và giờ của các điểm hiện tại chưa tăng nhất quán. Không tự thay đổi các điểm đã có.",[]);
        return new("ready","Có thể bổ sung điểm mà giữ nguyên lịch hiện tại.",[]);
    }
    public static StopAppendPlan Plan(Trip trip,BusRoute route,IReadOnlyList<TripStop> existing,IReadOnlyList<StopAppendRequest> requested,DateTime now) {
        var check=Check(trip,existing,now);if(check.Blocked)return check;
        var duration=TripStopService.Duration(trip);
        if(requested.Count==0)return new("complete","Không có điểm cần bổ sung.",[]);
        var working=existing.OrderBy(s=>s.StopOrder).ToList();var added=new List<TripStop>();
        if(working.Count==0) {
            added.Add(new(){TripId=trip.Id,StopOrder=0,Name="Điểm đầu · "+route.Origin,OffsetMinutes=0,CanPickup=true,IsActive=true,IsIllustrative=true});
            added.Add(new(){TripId=trip.Id,StopOrder=1000,Name="Điểm cuối · "+route.Destination,OffsetMinutes=duration,CanDropoff=true,IsActive=true,IsIllustrative=true});
            working.AddRange(added);
        }
        foreach(var request in requested.OrderBy(x=>x.OffsetMinutes)) {
            if(string.IsNullOrWhiteSpace(request.Name)||request.Name.Trim().Length>160||(request.Address?.Length??0)>500||
               request.OffsetMinutes<=0||request.OffsetMinutes>=duration||(!request.CanPickup&&!request.CanDropoff))
                return new("blocked","Tên, thời gian hoặc điều kiện đón/trả của điểm mới chưa hợp lệ.",[]);
            var sameName=working.Where(s=>StopPresentation.Key(s.Name)==StopPresentation.Key(request.Name)).ToList();
            if(sameName.Count>0) {
                if(sameName.Count!=1||sameName[0].OffsetMinutes!=request.OffsetMinutes||!sameName[0].IsActive||
                   (request.CanPickup&&!sameName[0].CanPickup)||(request.CanDropoff&&!sameName[0].CanDropoff)||
                   (request.Address!=null&&StopPresentation.Address(sameName[0].Address)!=StopPresentation.Address(request.Address)))
                    return new("blocked","Điểm "+StopPresentation.Name(request.Name)+" đã có cấu hình khác. Giữ nguyên điểm hiện tại để quản trị viên duyệt.",[]);
                continue;
            }
            if(working.Any(s=>s.OffsetMinutes==request.OffsetMinutes))
                return new("blocked","Mốc giờ của điểm "+StopPresentation.Name(request.Name)+" đã có điểm khác. Không tự ghi đè lịch riêng.",[]);
            var before=working.Where(s=>s.OffsetMinutes<request.OffsetMinutes).OrderBy(s=>s.OffsetMinutes).LastOrDefault();
            var after=working.Where(s=>s.OffsetMinutes>request.OffsetMinutes).OrderBy(s=>s.OffsetMinutes).FirstOrDefault();
            if(before==null||after==null||after.StopOrder-before.StopOrder<2)
                return new("blocked","Không còn khoảng thứ tự trống để chèn điểm "+StopPresentation.Name(request.Name)+". Không tự đánh lại thứ tự điểm cũ.",[]);
            if(working.Count>=20)return new("blocked","Lịch đã đủ 20 điểm; không tự bỏ điểm hiện có để chèn thêm.",[]);
            var row=new TripStop {TripId=trip.Id,StopOrder=before.StopOrder+(after.StopOrder-before.StopOrder)/2,
                Name=request.Name.Trim(),Address=string.IsNullOrWhiteSpace(request.Address)?null:request.Address.Trim(),OffsetMinutes=request.OffsetMinutes,
                CanPickup=request.CanPickup,CanDropoff=request.CanDropoff,IsActive=true,IsIllustrative=true};
            added.Add(row);working.Add(row);
        }
        return added.Count>0?new("ready","Đã kiểm tra các điểm còn thiếu.",added):new("complete","Các điểm đã có đầy đủ; không thay đổi lịch.",[]);
    }
}
