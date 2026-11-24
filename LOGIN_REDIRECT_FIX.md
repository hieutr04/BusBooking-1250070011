# Sửa đăng nhập theo workspace

## Nguyên nhân ở baseline

AccountController.Login cũ dùng LocalRedirect cho mọi local returnUrl, nếu không có thì Home/Index. Vì vậy Admin vào Home và returnUrl=/ cũng kéo Admin về Home. Admin view dùng _Layout khách hàng + menu ngang.

## Sửa tối thiểu ở server

- Controllers/AccountController.cs: giữ PasswordSignInAsync, IsActive, lockout, ModelState, POST/antiforgery/SignOut. Sau login thành công lấy roles bằng UserManager rồi LocalRedirect qua LoginDestination.
- Services/LoginDestination.cs mới: helper thuần, không DB/schema. Ưu tiên Admin→Staff→Driver→Customer, không ghi hoặc sửa dữ liệu role.
- GET Login của người đã đăng nhập cũng chọn workspace; Home thông thường vẫn render, không có global redirect.

| Vai trò ưu tiên | Mặc định | Các nhóm GET có thể phục hồi |
|---|---|---|
| Admin | /Admin | Admin index/danh mục/form GET/audit, Staff workflow, Bookings/Details GET, Account/Profile |
| Staff | /Staff | Staff workflow, Trips (tiếp tục bán/đổi), Bookings GET, Account/Profile |
| Driver | /Driver | Driver GET, Account/Profile |
| Customer | / | Home/Policy, Trips GET, Bookings GET, Account/Profile |

Admin returnUrl=/, /Home/Index hoặc chuyến công khai bị thay bằng /Admin. Admin chủ động bấm Xem website vẫn tới Home, không bị redirect lại. Staff không phục hồi Admin; Driver không phục hồi Staff/Trips; Customer không phục hồi workspace.

Url.IsLocalUrl tại controller kết hợp whitelist GET của helper. Chặn URL ngoài/protocol-relative, backslash/control characters, encoded path, dot segments, lặp slash, Login/Register/Logout, action POST và route không tồn tại trong whitelist. ~/ được chuẩn hóa thành /. Query trên đích hợp lệ được giữ (đặc biệt exchangeBookingId). ID dạng path phải là int dương hoặc GUID hợp lệ tùy action; query IDs và quyền sở hữu dữ liệu tiếp tục do controller/service kiểm tra, không coi whitelist là quyền truy cập.

Không sửa service booking, không migration/seed mới, không cho public register chọn role. POST logout vẫn có antiforgery. Authorization/ownership các controller giữ nguyên.

## Test

- Tests/BusBooking.RedirectTests: chạy chính helper bằng source link, không phụ thuộc SQL. Có role trống/đúng/sai đích, external/protocol-relative/loop/encoded/dot, ID sai, nhiều role, thứ tự ưu tiên.
- Tests/login-redirect-browser.mjs: login HTTP thật với cookies/CSRF cho bốn vai trò, các returnUrl và Login khi đã đăng nhập, Admin mở Home chủ động, POST logout + chặn workspace sau logout.
- Tests/browser.mjs: cập nhật helper chờ đúng workspace cho Staff/Driver/Admin; giữ các tests IDOR/CSRF/seat workflow.
- Login sai mật khẩu/IsActive/lockout: code không đổi, cần thêm kiểm runtime trên DB test như checklist TEST_RESULTS. Không biến giữ nguyên code thành bằng chứng đã chạy.

Tất cả test .NET/HTTP nói trên hiện CHƯA KIỂM CHỨNG vì CoreCLR không khởi tạo được và không có SQL Server. Source checks không thay build/RBAC thực tế.
