# BusGo — bản UI_CLEANUP_STOPS_REVIEWED, 03/11/2026

Sửa trực tiếp source STOPS_SEATS đính kèm. Giữ nguyên đề tài **XÂY DỰNG HỆ THỐNG QUẢN LÝ XE KHÁCH VÀ ĐẶT VÉ TRỰC TUYẾN TÍCH HỢP CƠ CHẾ GIỮ CHỖ THEO THỜI GIAN THỰC**, chuyên ngành **Thương mại điện tử**, ASP.NET Core MVC .NET 9, EF Core/Identity 9.0.9, SQL Server, SignalR và toàn bộ nghiệp vụ hiện có.

## Mở trên máy đã kết nối SQL

1. Dừng web (Shift + F5), sao lưu source và database hiện dùng bằng SSMS → Tasks → Back Up → Full. Không xóa DB hoặc lịch sử migration.
2. Giải nén, mở `BusBooking/BusBooking.sln`. Tên solution, project, namespace và UserSecretsId giữ nguyên. Giữ cấu hình SQL đang chạy thành công của máy bạn. ZIP không có mật khẩu SQL. Không đổi sang LocalDB hoặc chạy lại hướng dẫn xác thực Windows cũ.
3. Tại thư mục chứa `.sln`, kiểm tra bằng **cùng profile sẽ chạy website**:

```powershell
dotnet run --project BusBooking --launch-profile SqlServer -- --check-db
```

Khi báo `SQL Server connected. Pending migrations: 0`, chạy website:

```powershell
dotnet run --project BusBooking --launch-profile SqlServer
```

Hoặc chọn profile **SqlServer** trên thanh Visual Studio và F5. Profile Https vẫn được giữ nguyên; nếu dùng Https thì dùng cùng profile cho các lệnh kiểm tra/cập nhật. Không chép connection string cũ từ các ảnh/chat vào ZIP mới. `launchSettings.json` có thể ghi đè User Secrets: giữ thiết lập đã xác nhận đúng trên máy bạn.

## Ba nhóm thay đổi

- **BusGo:** header, tab, logo tự vẽ, favicon, sidebar, footer, ảnh vector sở hữu và copy động. Các tên kỹ thuật BusBooking/Auth cookie/Identity/route/role/migration không đổi.
- **Giao diện:** typography và bố cục chung trắng–xanh cho khách hàng và Admin/Staff/Driver; copy ngắn; bỏ diễn giải học tập/SQL/token. Giữ giá toàn chuyến, trạng thái thu/hoàn, giữ chỗ 300 giây, tối đa 6 ghế, lỗi thật và ghi công ảnh. Tên/địa chỉ lịch sử chỉ được trình bày lại; không sửa snapshot đã lưu.
- **Điểm Đà Lạt → TP. Hồ Chí Minh:** service chỉ-thêm dùng chung trip gate, giữ nguyên mọi điểm cũ và hold/vé; báo xung đột rõ. Admin có xem trước và thêm riêng một điểm. Chuyến mới đúng chiều 420 phút nhận cấu hình hiện có; lịch khác do Admin duyệt.

## Cập nhật các chuyến đang có

Không tự cập nhật DB của bạn chỉ bằng giải nén. Đọc `PICKUP_DROPOFF_GUIDE.md`, sao lưu rồi xem trước:

```powershell
dotnet run --project BusBooking --launch-profile SqlServer -- --review-dalat-stops
```

Xem `BusBooking/dalat-stops-preview.csv` hoặc phần **Admin → Chuyến → Điểm đón/trả**. Duyệt đúng chuyến rồi áp dụng từ Admin, hoặc cập nhật toàn bộ các chuyến đủ điều kiện:

```powershell
dotnet run --project BusBooking --launch-profile SqlServer -- --complete-dalat-stops
```

Báo cáo thật ở `BusBooking/dalat-stops-report.csv`. Chuyến bị chặn vẫn giữ nguyên; exit 2 nghĩa còn mục cần kiểm tra, không có nghĩa tất cả đã bổ sung. Lệnh chạy lại không tạo trùng điểm đã phù hợp. Không dùng seed để vượt khóa lịch.

## Database và kiểm chứng

Bản BusGo **không thêm migration hoặc đổi schema**. Ba migration từ đầu vào giữ nguyên byte. Nếu `--check-db` còn migration, dừng cập nhật điểm và xử lý đúng migration tăng thêm đang thiếu theo guide; không fake history/baseline, không chạy Initial chồng bảng.

Đã chạy kiểm tra source/cú pháp/asset và test JavaScript DOM; xem `TEST_RESULTS.md`. Restore/build và C# planner bị chặn bởi CoreCLR `0x8007000E`, exit 137. Chưa chạy SQL, MVC/Razor, HTTP, browser/responsive/concurrency; không có screenshot website chạy thật và không tuyên bố hết lỗi 500. ZIP dùng nhãn **REVIEWED**, không phải VERIFIED.

Tài liệu yêu cầu gốc được giữ nguyên; prompt lần này ở `Documentation/INPUT_BUSGO_REQUIREMENTS_ORIGINAL.txt`. Tài liệu bản trước ở `Documentation/history-before-busgo/` là lịch sử, không thay thế hướng dẫn hiện tại. Các báo cáo cũ trong baseline/validation không phải kết quả kiểm thử lần này. Nguồn ảnh/giấy phép ở `ASSET_SOURCES.md`; tài liệu học thuật khác vẫn có trong solution.
