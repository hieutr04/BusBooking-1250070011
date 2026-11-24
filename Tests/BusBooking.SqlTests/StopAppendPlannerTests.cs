using System.Text.Json;
using BusBooking.Models;
using BusBooking.Services;

// No SQL fixtures: boundary and non-mutation tests for the actual production planner.
public static class StopAppendPlannerTests {
 public static int Run() {
  var count=0;void Test(string name,Action test){test();count++;Console.WriteLine("PASS planner: "+name);}
  void Assert(bool ok,string reason){if(!ok)throw new Exception(reason);}
  var now=new DateTime(2026,11,3,0,0,0,DateTimeKind.Utc);
  Trip Trip()=>new(){Id=17,DepartureUtc=now.AddDays(1).AddHours(16),ArrivalUtc=now.AddDays(1).AddHours(23),Revision=29};
  var route=new BusRoute{Origin="Đà Lạt",Destination="TP. Hồ Chí Minh"};
  List<TripStop> Ends()=>[new(){Id=101,TripId=17,StopOrder=0,Name="Điểm đầu · Đà Lạt",CanPickup=true,IsIllustrative=false},new(){Id=102,TripId=17,StopOrder=1000,Name="Điểm cuối · TP. Hồ Chí Minh",OffsetMinutes=420,CanDropoff=true}];
  Test("insert two without changing existing IDs/order/flags/name/address",()=>{
   var rows=Ends();rows[0].Address="Địa chỉ riêng";var before=JsonSerializer.Serialize(rows);
   var p=StopAppendPlanner.Dalat(Trip(),route,rows,now);
   Assert(p.Ready&&p.NewStops.Count==2,"missing additions");Assert(before==JsonSerializer.Serialize(rows),"existing entities mutated");
   Assert(p.NewStops.Select(x=>x.OffsetMinutes).SequenceEqual(new[]{90,180}),"wrong reverse offsets");
   Assert(p.NewStops.All(x=>x.Id==0&&x.TripId==17&&x.IsIllustrative&&x.Address==null&&x.CanPickup&&x.CanDropoff),"false provenance/address/flags");
   var repeat=StopAppendPlanner.Dalat(Trip(),route,rows.Concat(p.NewStops).ToList(),now);Assert(repeat.Status=="complete"&&repeat.NewStops.Count==0,"second run duplicates");
  });
  Test("custom stop preserved and gaps used without renumbering",()=>{
   var rows=Ends();rows.Add(new(){Id=103,TripId=17,StopOrder=100,Name="Điểm riêng",OffsetMinutes=60,IsIllustrative=false});
   var before=JsonSerializer.Serialize(rows);var p=StopAppendPlanner.Dalat(Trip(),route,rows,now);
   Assert(p.Ready&&p.NewStops.All(x=>x.StopOrder>100&&x.StopOrder<1000),"not using empty gaps");Assert(before==JsonSerializer.Serialize(rows),"custom stop changed");
  });
  Test("old teaching prefix matches existing point without erasing provenance",()=>{
   var rows=Ends();rows.Add(new(){Id=103,TripId=17,StopOrder=200,Name="Điểm học tập · Đức Trọng",OffsetMinutes=90,CanPickup=true,CanDropoff=true});
   var p=StopAppendPlanner.Dalat(Trip(),route,rows,now);Assert(p.NewStops.Count==1&&p.NewStops[0].Name=="Bảo Lộc","duplicate prefixed name");Assert(rows[2].Name.StartsWith("Điểm học tập"),"rewrote raw name");
  });
  Test("conflicting configured time is blocked atomically",()=>{
   var rows=Ends();rows.Add(new(){Id=103,TripId=17,StopOrder=300,Name="Bảo Lộc",OffsetMinutes=210,CanPickup=true,CanDropoff=true});
   var p=StopAppendPlanner.Dalat(Trip(),route,rows,now);Assert(p.Blocked&&p.NewStops.Count==0,"partially proposed conflicting schedule");
  });
  Test("occupied offset or order gap blocks all additions",()=>{
   var rows=Ends();rows.Add(new(){Id=103,TripId=17,StopOrder=10,Name="Điểm riêng",OffsetMinutes=90});Assert(StopAppendPlanner.Dalat(Trip(),route,rows,now).Blocked,"overwrote offset");
   rows=Ends();rows.Add(new(){Id=103,TripId=17,StopOrder=1,Name="Điểm riêng",OffsetMinutes=100});var p=StopAppendPlanner.Dalat(Trip(),route,rows,now);Assert(p.Blocked&&p.NewStops.Count==0,"renumbered existing stop");
  });
  Test("past, boarding, wrong duration and reverse route not applied",()=>{
   var t=Trip();t.DepartureUtc=now;Assert(StopAppendPlanner.Dalat(t,route,Ends(),now).Blocked,"past accepted");
   t=Trip();t.Status=TripStatus.Boarding;Assert(StopAppendPlanner.Dalat(t,route,Ends(),now).Blocked,"boarding accepted");
   t=Trip();t.ArrivalUtc=t.ArrivalUtc.AddMinutes(1);Assert(StopAppendPlanner.Dalat(t,route,Ends(),now).Blocked,"interpolated timetable");
   Assert(StopAppendPlanner.Dalat(Trip(),new(){Origin="TP.HCM",Destination="Đà Lạt"},Ends(),now).Status=="not-applicable","wrong direction");
  });
  Test("foreign IDs and invalid existing ordering blocked",()=>{
   var rows=Ends();rows[0].TripId=18;Assert(StopAppendPlanner.Dalat(Trip(),route,rows,now).Blocked,"foreign trip");
   rows=Ends();rows.Add(new(){TripId=17,StopOrder=10,OffsetMinutes=450,Name="Ngoài giờ"});Assert(StopAppendPlanner.Dalat(Trip(),route,rows,now).Blocked,"invalid order");
  });
  Test("empty schedule gains endpoints and points once",()=>{
   var t=Trip();var p=StopAppendPlanner.Dalat(t,route,[],now);Assert(p.Ready&&p.NewStops.Count==4,"empty schedule");
   Assert(TripStopService.AtUtc(t,p.NewStops.Single(x=>x.StopOrder==1000))==t.ArrivalUtc,"end point differs");
   var mid=p.NewStops.Single(x=>x.Name=="Đức Trọng");Assert(BusinessTime.Text(TripStopService.AtUtc(t,mid))=="05/11/2026 00:30","UTC+7 midnight");
  });
  Test("presentation cleans labels without rewriting source",()=>{
   var raw="Điểm học tập · Đức Trọng";Assert(StopPresentation.Name(raw)=="Đức Trọng"&&raw.StartsWith("Điểm học tập"),"presentation");
   Assert(StopPresentation.Address("  ")==null&&StopPresentation.Duration(440)=="7 giờ 20 phút","format");
   Assert(StopAppendPlanner.DalatRoute(new(){Origin="Thành phố Đà Lạt",Destination="TP.HCM"}),"route alias");
  });
  Test("audit labels do not rewrite stored metadata",()=>{
   var row=new AuditEntry{Action="TripStopsAppended",Detail="insert-only; source=configured-stops; count=2"};
   Assert(AuditPresentation.Detail(row)=="Thêm 2 điểm; giữ nguyên lịch và thông tin đặt vé hiện có.","audit display");
   Assert(row.Detail=="insert-only; source=configured-stops; count=2","audit mutated");
  });
  Console.WriteLine($"Planner cases: {count}");return count;
 }
}
