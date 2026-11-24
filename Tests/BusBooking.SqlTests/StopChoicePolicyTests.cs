using BusBooking.Models;
using BusBooking.Services;
using System.Text.Json;

public static class StopChoicePolicyTests {
 public static void Run() {
  void Assert(bool ok,string message){if(!ok)throw new Exception(message);}
  void Reject(Action action){try{action();throw new Exception("Expected rejection");}catch(BusinessException){}}
  var now=new DateTime(2026,11,3,0,0,0,DateTimeKind.Utc);var route=new BusRoute{Origin="Đà Lạt",Destination="TP. Hồ Chí Minh"};
  var trip=new Trip{Id=28,DepartureUtc=now.AddDays(1).AddHours(16),ArrivalUtc=now.AddDays(1).AddHours(23),Fare=320000,Revision=13};
  var rows=new List<TripStop>{new(){Id=1,TripId=28,StopOrder=0,Name="Điểm đầu · Đà Lạt",CanPickup=true},new(){Id=2,TripId=28,StopOrder=1000,Name="Điểm cuối · TP. Hồ Chí Minh",OffsetMinutes=420,CanDropoff=true},new(){Id=3,TripId=28,StopOrder=500,Name="Điểm học tập · Đức Trọng",OffsetMinutes=90,CanPickup=true,CanDropoff=true}};
  Assert(StopChoicePolicy.ForNew(trip,route,rows).Count==2,"sample locality exposed for new bookings");
  Reject(()=>StopChoicePolicy.SelectNew(trip,route,rows,3,2));
  rows[2].IsIllustrative=false;Reject(()=>StopChoicePolicy.SelectNew(trip,route,rows,3,2));rows[2].IsIllustrative=true;
  var held=new SeatHold{TripId=28,PickupStopId=3,DropoffStopId=2};
  Assert(StopChoicePolicy.SelectHeld(trip,route,rows,held,3,2).Pickup.Id==3,"existing holder lost point");
  var before=JsonSerializer.Serialize(rows);
  SpecificStopRequest Pick()=>new("Điểm hẹn A","Địa chỉ kiểm thử riêng",0,true,false,"Đà Lạt","Nguồn và xác nhận khai thác của fixture",true);
  SpecificStopRequest Drop()=>new("Điểm hẹn B","Địa chỉ kiểm thử riêng B",420,false,true,"TP. Hồ Chí Minh","Nguồn và xác nhận khai thác của fixture",true);
  var plan=SpecificStopPlanner.Plan(trip,route,rows,[Pick(),Drop()],now);Assert(plan.Ready&&plan.NewStops.Count==2,"cannot add endpoint alternatives");
  Assert(before==JsonSerializer.Serialize(rows),"mutated old rows");
  for(int i=0;i<plan.NewStops.Count;i++)plan.NewStops[i].Id=100+i;
  var all=rows.Concat(plan.NewStops).ToList();
  var selected=StopChoicePolicy.SelectNew(trip,route,all,null,null);
  Assert(selected.Pickup.Id==100&&selected.Dropoff.Id==101,"wrong default area/endpoint");
  Assert(StopChoicePolicy.SelectHeld(trip,route,all,held,3,2).Pickup.Id==3,"grandfather choice lost after update");
  Reject(()=>StopChoicePolicy.SelectNew(trip,route,all,1,2));
  Reject(()=>StopChoicePolicy.SelectHeld(trip,route,all,new(){TripId=28,PickupStopId=1,DropoffStopId=2},3,2));
  Assert(SpecificStopPlanner.Plan(trip,route,all,[Pick(),Drop()],now).Status=="complete","second run duplicates");
  Assert(SpecificStopPlanner.Plan(trip,route,rows,[Pick() with{OperatorConfirmed=false}],now).Blocked,"no operator review");
  Assert(SpecificStopPlanner.Plan(trip,route,rows,[Pick() with{Area="Nha Trang"}],now).Blocked,"wrong origin area");
  Assert(SpecificStopPlanner.Plan(trip,route,rows,[Pick() with{Name="Đức Trọng"}],now).Blocked,"locality treated as concrete point");
  Assert(SpecificStopPlanner.Plan(trip,route,rows,[Drop() with{CanPickup=true}],now).Blocked,"endpoint opened both roles");
  Assert(SpecificStopPlanner.Plan(trip,route,rows,[Pick() with{OffsetMinutes=60,CanPickup=false,CanDropoff=false}],now).Blocked,"neither operation enabled");
  Assert(SpecificStopPlanner.Plan(trip,route,all,[Drop() with{Address="Khác cấu hình"}],now).Blocked,"custom address overwritten");
  Assert(TripStopService.AtUtc(trip,selected.Dropoff)==trip.ArrivalUtc,"arrival endpoint shifted");
  var reverse=new BusRoute{Origin="TP. Hồ Chí Minh",Destination="Đà Lạt"};Assert(SpecificStopPlanner.Plan(trip,reverse,rows,[Pick()],now).Blocked,"reverse schedule ignored area");
  var contact=new ContactOptions{SupportWebsite="javascript:alert(1)",SupportPhone="-------",SupportEmail="not-an-email"};
  Assert(contact.ValidWebsite==null&&contact.ValidPhone==null&&contact.ValidEmail==null,"invalid contact links exposed");
  contact.SupportWebsite="https://example.test/help";contact.SupportPhone="+84 900 000 000";contact.SupportEmail="operator@example.test";
  Assert(contact.ValidWebsite=="https://example.test/help"&&contact.PhoneLink=="+84900000000"&&contact.ValidEmail=="operator@example.test","valid configured contact rejected");
  Console.WriteLine("PASS StopChoicePolicy/SpecificStopPlanner: legacy eligibility, retained holds, new endpoints, idempotency, area/source/flags/time and no mutation");
 }
}
