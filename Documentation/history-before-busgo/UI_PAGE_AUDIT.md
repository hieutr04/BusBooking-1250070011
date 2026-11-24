## Audit bổ sung — 02/11/2026

| Trang | Thay đổi | Phần giữ nguyên |
|---|---|---|
| Home | Hero và ảnh đích, nguồn ảnh, CTA Liên hệ | Search GET, origin/destination/date, tuyến backend |
| Trips/Index | Thumbnail coach + ghi công | Bộ lọc, đổi vé, thời gian/giá/ghế và Details link |
| Account Login/Register/Profile | Ảnh desktop | Form, model binding và backend byte-identical |
| Shared public/workspace | Link Liên hệ/Nguồn ảnh, sidebar Admin | Những mục cũ, menu, account, POST logout |
| Contact/Index | Hai cột, form client/server | Module mới trong phạm vi cho phép |
| AdminContacts/Index,Details | List/chi tiết/mark-read | Chỉ Admin, layout cũ, không dashboard ảnh quảng cáo |
| Photos/Index | Ghi công và giấy phép | Không đọc dữ liệu cá nhân |
| Trips/Details, Bookings, Staff, Driver, Policy | Không sửa nội dung | Giữ toàn bộ DOM, ghế/API/form/nghiệp vụ |

CSS có breakpoint tương ứng, nhưng viewport/browser thực chưa được chạy. Các audit trang lần trước nằm phía dưới.

---

# Audit baseline và bao phủ trang/action

ZIP đầu vào: QuanLyXeKhach_DatVe_GiuCho_REALTIME_REVIEWED(2).zip;116 tệp;31 Razor. Baseline chưa được build/run trong môi trường nâng cấp. Lời xác nhận người dùng trước đây đã chạy SQL và ảnh cũ không được dùng làm bằng chứng runtime của lần sửa này.

Source đã rà Account/Admin/Bookings/Driver/Home/SeatsApi/Staff/Trips, Data/AppDbContext/Seed/migrations, các service/worker/Hub,31 Razor và CSS/JS. Hash từng tệp đầu vào ở Documentation/baseline/SOURCE_BASELINE_SHA256.json. Có34 Razor sau nâng cấp: giữ31, thêm _AdminLayout, _WorkspaceSidebar, _Icon. Không xóa view/route.

| Trang/action | Role | View | Thay đổi UI / chức năng | Test cần chạy | Kết quả |
|---|---|---|---|---|---|
| `Account/Login` | Guest/đã đăng nhập | `BusBooking/Views/Account/Login.cshtml` | Đăng nhập Identity, chuyển hướng theo vai trò; form rõ nhãn, autocomplete, hiện/ẩn mật khẩu | GET/POST, validation, CSRF và role; UI viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Account/Profile` | Guest/đã đăng nhập | `BusBooking/Views/Account/Profile.cshtml` | Cập nhật tên/điện thoại; cùng binding, layout theo vai trò | GET/POST, validation, CSRF và role; UI viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Account/Register` | Guest/đã đăng nhập | `BusBooking/Views/Account/Register.cshtml` | Đăng ký Customer; form validation và trợ giúp mật khẩu | GET/POST, validation, CSRF và role; UI viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Admin/Audit` | Admin | `BusBooking/Views/Admin/Audit.cshtml` | Nhật ký từ SQL; bảng và giới hạn hiển thị rõ | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Admin/Index` | Admin | `BusBooking/Views/Admin/Index.cshtml` | KPI/filter/Chart.js/ledger từ ReportService; bố cục dashboard và empty state theo dữ liệu | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Admin/LayoutEdit` | Admin | `BusBooking/Views/Admin/LayoutEdit.cshtml` | Thêm/sửa layout; preview tạm theo input, không đổi quy tắc lưu | GET/POST, validation, CSRF và role; UI viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Admin/Layouts` | Admin | `BusBooking/Views/Admin/Layouts.cshtml` | Cấu hình ghế đang lưu; card, badge và mã ghế từ model | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Admin/RouteEdit` | Admin | `BusBooking/Views/Admin/RouteEdit.cshtml` | Thêm/sửa tuyến; fieldset, validation và savebar | GET/POST, validation, CSRF và role; UI viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Admin/Routes` | Admin | `BusBooking/Views/Admin/Routes.cshtml` | Danh sách tuyến hoạt động; toolbar, số dòng thực và bảng | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Admin/TripEdit` | Admin | `BusBooking/Views/Admin/TripEdit.cshtml` | Tạo/sửa chuyến và phân công; đầy đủ field giờ Việt Nam, giá, tài xế/nhân viên | GET/POST, validation, CSRF và role; UI viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Admin/Trips` | Admin | `BusBooking/Views/Admin/Trips.cshtml` | Chuyến/giá/phân công/sales/cancel; bảng và hành động hiện có | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Admin/Users` | Admin | `BusBooking/Views/Admin/Users.cshtml` | Vai trò/IsActive; form/bảng và xác nhận thay quyền | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Admin/VehicleEdit` | Admin | `BusBooking/Views/Admin/VehicleEdit.cshtml` | Thêm/sửa xe; nhóm field và giữ select layout/IsActive | GET/POST, validation, CSRF và role; UI viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Admin/Vehicles` | Admin | `BusBooking/Views/Admin/Vehicles.cshtml` | Danh sách xe/layout; toolbar, trạng thái và bảng | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Bookings/Details` | Customer hoặc Staff/Admin khi được phép | `BusBooking/Views/Bookings/Details.cshtml` | Chi tiết/hành khách/thu-hoàn/đổi-hủy; ledger tổng hợp thật và cùng điều kiện action | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Bookings/Index` | Customer hoặc Staff/Admin khi được phép | `BusBooking/Views/Bookings/Index.cshtml` | Vé chủ tài khoản; thẻ mã vé, hành trình, trạng thái tiền riêng | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Driver/Index` | Driver | `BusBooking/Views/Driver/Index.cshtml` | Lịch được giao; card gọn và links danh sách hành khách | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Home/Index` | Public | `BusBooking/Views/Home/Index.cshtml` | Form GET tìm chuyến, tuyến model và hướng dẫn; hero/form/CTA trắng xanh | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Home/Policy` | Public | `BusBooking/Views/Home/Policy.cshtml` | Chính sách gốc; intro, phân đoạn và bố cục dễ đọc | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Dùng chung: Passengers` | Theo view gọi / role | `BusBooking/Views/Shared/Passengers.cshtml` | Hành khách theo quyền, Driver progression; workspace và bảng | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Dùng chung: Problem` | Theo view gọi / role | `BusBooking/Views/Shared/Problem.cshtml` | Hiển thị lỗi theo status/request ID; heading phù hợp và links GET | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Dùng chung: _AdminNav` | Theo view gọi / role | `BusBooking/Views/Shared/_AdminNav.cshtml` | Context quản trị gọn; sidebar mới sở hữu điều hướng | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Dùng chung: _Layout` | Theo view gọi / role | `BusBooking/Views/Shared/_Layout.cshtml` | Shell public header/footer, menu mobile/account và Scripts | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Dùng chung: _StaffNav` | Theo view gọi / role | `BusBooking/Views/Shared/_StaffNav.cshtml` | Context phòng vé gọn; sidebar theo role sở hữu điều hướng | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Staff/Index` | Staff/Admin | `BusBooking/Views/Staff/Index.cshtml` | Tra cứu toàn DB trước Take, bán hỗ trợ; toolbar, form và bảng | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Staff/Requests` | Staff/Admin | `BusBooking/Views/Staff/Requests.cshtml` | Xử lý yêu cầu/từ chối; card có lý do và action cũ | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Staff/Trips` | Staff/Admin | `BusBooking/Views/Staff/Trips.cshtml` | Danh sách chuyến để xem hành khách; bảng gọn và links thật | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Trips/Details` | Theo view gọi / role | `BusBooking/Views/Trips/Details.cshtml` | Sơ đồ/hold/countdown/confirm; giữ contract IDs, form rõ và no-JS notice | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức; giữ query đổi vé, seat IDs/reconnect | CHƯA KIỂM CHỨNG runtime |
| `Trips/Index` | Theo view gọi / role | `BusBooking/Views/Trips/Index.cshtml` | Tìm chuyến mở bán; form giữ query, card giá/ghế/giờ và empty state | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức; giữ query đổi vé, seat IDs/reconnect | CHƯA KIỂM CHỨNG runtime |
| `Views/_ViewImports` | Theo view gọi / role | `BusBooking/Views/_ViewImports.cshtml` | Giữ imports/tag helpers cho mọi view; không đổi nghiệp vụ | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |
| `Views/_ViewStart` | Theo view gọi / role | `BusBooking/Views/_ViewStart.cshtml` | Chọn layout theo controller và vai trò, không redirect toàn cục | GET, dữ liệu/empty, quyền, keyboard và viewport 5 mức | CHƯA KIỂM CHỨNG runtime |

## Action không có View riêng

SeatsApi Snapshot/hold/release/confirm vàcsrf giữ API contract. Admin POST Save/Sales/CancelTrip/UserRole; Staff Money/Cancel/Reject; Driver Progress; Bookings Request; Account Logout vẫn dùng authorization/antiforgery/service cũ. Staff/Sell redirect Trips, Driver/Details dùng Shared/Passengers; Home/Status/Error dùng Shared/Problem. Account/Login GET/POST thay redirect như LOGIN_REDIRECT_FIX. Không thêm action nghiệp vụ.

Backend bảo vệ so bằng hash: Models,Data/migrations,SeedData,Program,BookingService,CatalogService,ReportService,RealtimePublisher/Metrics,worker,Hub,SeatsApi và các controller khác Account được giữ byte-for-byte. Báo cáo core-preservation.json có kết quả cụ thể.
