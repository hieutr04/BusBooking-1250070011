using System.Text.Json;
using BusBooking.Services;
var cases=new List<object>();int failed=0;
void Check(string label,string[] roles,string? input,string expected) {
    var actual=LoginDestination.Select(roles,input);var passed=actual==expected;if(!passed)failed++;
    cases.Add(new{label,roles,input,expected,actual,status=passed?"PASS":"FAIL"});
}
foreach(var role in new[]{"Admin","Staff","Driver","Customer"}) {
    var home=role switch {"Admin"=>"/Admin","Staff"=>"/Staff","Driver"=>"/Driver",_=>"/"};
    foreach(var input in new string?[]{null,"","https://evil.test","//evil.test","/\\evil.test","/Account/Login","/Account/Register","/Account/Logout","/%2f%2fevil.test","/Admin/../Home","/Trips//Details/1","/Home#x","/Trips/Details/1\r\nX:test"})Check("reject_or_default",[role],input,home);
}
Check("admin_home_ignored",["Admin"],"/","/Admin");Check("admin_home_controller_ignored",["Admin"],"/Home/Index","/Admin");
Check("admin_personal_list_ignored",["Admin"],"/Bookings","/Admin");
Check("admin_public_trip_ignored",["Admin"],"/Trips/Details/3","/Admin");
Check("admin_workspace",["Admin"],"/Admin/Trips","/Admin/Trips");Check("admin_staff_workspace",["Admin"],"/Staff?\u0071=VX1","/Staff?q=VX1");
Check("no_post_sales_redirect",["Admin"],"/Admin/Sales/1","/Admin");
Check("staff_workspace",["Staff"],"/Staff/Requests","/Staff/Requests");Check("staff_exchange",["Staff"],"/Trips/Details/3?exchangeBookingId=123","/Trips/Details/3?exchangeBookingId=123");
Check("staff_no_admin",["Staff"],"/Admin","/Staff");Check("driver_no_staff",["Driver"],"/Staff","/Driver");Check("driver_details",["Driver"],"/Driver/Details/2","/Driver/Details/2");
Check("customer_no_admin",["Customer"],"/Admin","/");Check("customer_trip_restored",["Customer"],"/Trips/Details/42?exchangeBookingId=keep","/Trips/Details/42?exchangeBookingId=keep");
Check("app_relative",["Customer"],"~/Trips/Details/42","/Trips/Details/42");Check("malformed_id",["Customer"],"/Trips/Details/nope","/");
Check("multi_admin_priority",["Customer","Driver","Staff","Admin"],"/","/Admin");Check("multi_staff_priority",["Customer","Driver","Staff"],null,"/Staff");Check("multi_driver_priority",["Customer","Driver"],null,"/Driver");
Check("case_insensitive",["Admin"],"/admin/trips","/admin/trips");
Console.WriteLine(JsonSerializer.Serialize(new{kind="PURE_REDIRECT_POLICY_NOT_SQL_OR_HTTP",failed,cases},new JsonSerializerOptions{WriteIndented=true}));
Environment.ExitCode=failed==0?0:1;
