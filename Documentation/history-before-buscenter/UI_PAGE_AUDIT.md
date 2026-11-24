# BusGo — rà trang ở mức source

Tất cả View dưới đây dùng layout/style chung (hoặc partial). Copy kỹ thuật/nhãn cũ đã rà ở source. Đây **không phải** PASS HTTP/Razor/browser/responsive; năm viewport 375/390/768/1024/1440px phải kiểm trên app chạy thật.

| View | Xử lý | Runtime |
|---|---|---|
| `Account/Login.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Account/Profile.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Account/Register.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Admin/Audit.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Admin/Index.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Admin/LayoutEdit.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Admin/Layouts.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Admin/RouteEdit.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Admin/Routes.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Admin/TripEdit.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Admin/TripStops.cshtml` | Màn hình duyệt và bổ sung điểm riêng, khóa sửa cũ | CHƯA KIỂM CHỨNG |
| `Admin/Trips.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Admin/Users.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Admin/VehicleEdit.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Admin/Vehicles.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `AdminContacts/Details.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `AdminContacts/Index.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Bookings/Details.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Bookings/Index.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Contact/Index.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Driver/Index.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Home/Index.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Home/Policy.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Photos/Index.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Shared/Passengers.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Shared/Problem.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Shared/_AdminLayout.cshtml` | Thành phần dùng chung/đầu ra lịch sử/điều hướng; giữ hợp đồng form | CHƯA KIỂM CHỨNG |
| `Shared/_AdminNav.cshtml` | Thành phần dùng chung/đầu ra lịch sử/điều hướng; giữ hợp đồng form | CHƯA KIỂM CHỨNG |
| `Shared/_BookingStops.cshtml` | Thành phần dùng chung/đầu ra lịch sử/điều hướng; giữ hợp đồng form | CHƯA KIỂM CHỨNG |
| `Shared/_Icon.cshtml` | Thành phần dùng chung/đầu ra lịch sử/điều hướng; giữ hợp đồng form | CHƯA KIỂM CHỨNG |
| `Shared/_Layout.cshtml` | Thành phần dùng chung/đầu ra lịch sử/điều hướng; giữ hợp đồng form | CHƯA KIỂM CHỨNG |
| `Shared/_Photo.cshtml` | Thành phần dùng chung/đầu ra lịch sử/điều hướng; giữ hợp đồng form | CHƯA KIỂM CHỨNG |
| `Shared/_StaffNav.cshtml` | Thành phần dùng chung/đầu ra lịch sử/điều hướng; giữ hợp đồng form | CHƯA KIỂM CHỨNG |
| `Shared/_WorkspaceSidebar.cshtml` | Thành phần dùng chung/đầu ra lịch sử/điều hướng; giữ hợp đồng form | CHƯA KIỂM CHỨNG |
| `Staff/Index.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Staff/Requests.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Staff/Trips.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Trips/Details.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `Trips/Index.cshtml` | Dùng dữ liệu/ảnh/biểu mẫu hiện có, style chung; copy gọn | CHƯA KIỂM CHỨNG |
| `_ViewImports.cshtml` | Thành phần dùng chung/đầu ra lịch sử/điều hướng; giữ hợp đồng form | CHƯA KIỂM CHỨNG |
| `_ViewStart.cshtml` | Thành phần dùng chung/đầu ra lịch sử/điều hướng; giữ hợp đồng form | CHƯA KIỂM CHỨNG |
