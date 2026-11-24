# Chạy kiểm thử bản Ảnh & Liên hệ

## Kiểm tra source không đụng database

```powershell
python Tests/source-check.py
python Tests/images-contact-check.py --baseline "DUONG_DAN_ZIP_DAU_VAO.zip"
```

Baseline là ZIP UI_UPGRADED_REVIEWED(2) bạn đã gửi, không được đóng gói lồng trong bản mới. Có thể bỏ --baseline để chỉ kiểm file/tài nguyên. Các lệnh này không compile C#/Razor, không xác nhận SQL hay browser.

## Chuẩn bị môi trường thực

Giữ bản đang chạy và cấu hình thật. Tạo database SQL Server TEST riêng/khôi phục bản sao đã loại dữ liệu cá nhân, dùng cấu hình của bản copy source test (không thay cấu hình bản thật). Áp dụng AddContactMessages trên TEST, chuẩn bị 4 tài khoản test đang có. Không tự seed vào database vận hành. Từ thư mục solution, chạy:

```powershell
dotnet --info
dotnet tool restore
dotnet restore BusBooking.sln
dotnet build BusBooking.sln
dotnet ef migrations has-pending-model-changes --project BusBooking
dotnet run --project BusBooking --launch-profile Https -- --check-db
dotnet run --project BusBooking --launch-profile SqlServer
```

Nếu chạy migration bằng EF, tài khoản triển khai test phải có quyền DDL. Có thể dùng SQL companion qua SSMS quản trị thay vì cấp DDL cho login vận hành. Lệnh model-diff chỉ đọc model, không được dùng tạo lại Initial để vượt cảnh báo.

## Browser/HTTP với bản ghi SQL thật

Ở terminal khác, vào Tests, `npm install`, `npx playwright install chromium`. Giữ server TEST đang chạy, đặt các biến sau trong terminal test:

```powershell
$env:APP_URL='http://localhost:5190'
$env:BB_ALLOW_TEST_WRITES='YES'
$env:BB_ROLE_PASSWORD='<mat khau cac tai khoan test hien co>'
node contact-browser.mjs
```

Script chỉ cho hostname localhost/127.0.0.1/::1, tạo một liên hệ tổng hợp với email example.invalid, cần các tài khoản admin/staff/driver/customer@busbooking.test có cùng mật khẩu test bạn cấu hình. Không gửi email và không xóa bản ghi cũ. Không chạy trên database có nội dung liên hệ thật khi chụp screenshot Admin. Có thể còn một dòng test có Subject CONTACT-TEST-* để đối chiếu trong SQL TEST.

Script kiểm hợp lệ/lưu/PRG, replay cùng token đúng một dòng, replay đổi nội dung bị 409, validation, anti-forgery, giả token, field quá dài, body ngắn có padding, quota request thứ 9, Admin xem/mark-read, HTML encode, chặn 3 vai trò khác và Guest. Nó tiếp tục kiểm 375/390/768/1024/1440px, ảnh local khi chặn network bên ngoài, fallback, Reduce Motion; lưu kết quả và screenshot thật vào Tests/TestResults/contact/ sau khi chạy trên ứng dụng thực. Vì quota 8/10 phút theo IP, chạy lại script cần đợi cửa sổ quota hoặc khởi động lại bản test; không giảm quota của bản vận hành để qua test.

## Các trường hợp cần kiểm thủ công riêng

- SQL TEST không sẵn sàng hoặc thiếu ContactMessages: gửi dữ liệu hợp lệ, phải nhận 503, giữ trường nhập và không có thông báo thành công; không gây lỗi trên database thật để thử.
- POST trên 64 KiB hoặc hơn 12 field: bị chặn; thử ngoài cửa sổ quota, trực tiếp HTTP để không bị maxlength của browser cản trước. Chuỗi script/XSS phải hiển thị nguyên văn ở Admin, không thực thi.
- Keyboard Tab/Shift+Tab/Enter/Escape, focus/summary, mobile menu, sidebar, màn hình zoom; nguồn/giấy phép ngay tại ảnh và /nguon-anh.
- Route mới chưa có ảnh: fallback trung tính; thay tên tuyến/ID không được làm gán ảnh sai địa phương.
- Tắt mạng Internet bên ngoài nhưng giữ localhost hoạt động, tải lại trang phải còn ảnh local. Không dùng browser offline tuyệt đối để kỳ vọng localhost vẫn tải được tài liệu HTTP.
- Luồng đặt vé/đổi/hủy/thu-hoàn, hai tài khoản tranh ghế, refresh/reconnect/restart, hết hạn 300s và tải 20/50/100: chạy lại các test SQL/concurrency/redirect/browser cũ trong ZIP. Không coi việc hash file cũ không đổi là test runtime PASS.

Không có kết quả PASS cho các mục này nếu chưa chạy. Điền ngày, lệnh và bằng chứng thực tế vào TEST_RESULTS sau thử trên máy có môi trường phù hợp.
