# Bus Center — BusBooking

Hệ thống quản lý xe khách và đặt vé trực tuyến tích hợp cơ chế giữ chỗ theo thời gian thực.
Mã nguồn ASP.NET Core MVC, SQL Server, EF Core, Identity và SignalR. Tên hiển thị Bus Center; tên solution và namespace BusBooking.

## Thông tin nộp bài

- Sinh viên: Trần Trung Hiếu — MSSV 1250070011 (theo tên báo cáo hiện có; kiểm tra lại trước khi nộp).
- GitHub: https://github.com/hieutr04/BusBooking-1250070011.
- GVHD: ThS. Từ Thanh Trí.
- Chưa tạo repository hoặc mời GVHD bằng bộ hồ sơ này. Sinh viên làm theo file Word đi kèm.

## Thành phần

| Đường dẫn | Nội dung |
| --- | --- |
| `BusBooking.sln` | Solution chính |
| `BusBooking/` | Mã C#, Razor, JavaScript, CSS, ảnh, thư viện phía trình duyệt |
| `BusBooking/Data/Migrations/` | Lịch sử migration và snapshot EF Core |
| `Database/` | SQL tham khảo, hướng dẫn CSDL và tái tạo dữ liệu |
| `.config/dotnet-tools.json` | Công cụ dotnet-ef 9.0.9 |
| `ENVIRONMENT.md`, `DEPENDENCIES.csv` | Môi trường, phiên bản thư viện và lệnh khôi phục |
| `Scripts/`, `Tests/` | Script hỗ trợ và mã kiểm thử hiện có |
| `Documentation/` | Tài liệu thiết kế, lịch sử và hướng dẫn chi tiết |

## Cài đặt trên Windows

1. Cài .NET SDK 9.0.305 hoặc bản 9.0 mới hơn được `global.json` chấp nhận (`rollForward: latestFeature`), SQL Server và SSMS. Có thể dùng Visual Studio hỗ trợ .NET 9 hoặc terminal. Node.js/npm chỉ cần cho kiểm thử trình duyệt.
2. Mở PowerShell tại thư mục chứa `BusBooking.sln`.

```powershell
dotnet --version
dotnet restore BusBooking.sln
dotnet tool restore
dotnet build BusBooking.sln --no-restore
```

3. Trong SSMS, xác nhận instance SQL Server hoạt động. Tạo database mới `BusBookingStudy_Submission` nếu tài khoản migration không có quyền CREATE DATABASE. Thay `.\SQLEXPRESS` bằng instance thực tế; ví dụ dưới dùng Windows Authentication.

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\SQLEXPRESS;Database=BusBookingStudy_Submission;Trusted_Connection=True;TrustServerCertificate=True" --project BusBooking
dotnet ef database update --project BusBooking
dotnet run --project BusBooking --launch-profile SqlServer -- --check-db
```

Kết quả kiểm tra cần có `SQL Server connected. Pending migrations: 0`. `TrustServerCertificate=True` chỉ phục vụ môi trường học tập cục bộ. Nếu dùng SQL Authentication, lưu thông tin thật trong User Secrets hoặc biến môi trường `ConnectionStrings__DefaultConnection`, không ghi mật khẩu vào GitHub. Biến môi trường có thể ghi đè User Secrets.

4. Chỉ trên database mới dùng để chấm/demo, tạo dữ liệu mẫu:

```powershell
dotnet run --project BusBooking --launch-profile SqlServer -- --seed
dotnet run --project BusBooking --launch-profile SqlServer
```

Mở `http://localhost:5190`. Profile `Https`: `https://localhost:7190` (cần chứng chỉ development, có thể chạy `dotnet dev-certs https --trust` trên máy cá nhân). Chạy camera QR qua localhost hoặc HTTPS và cho phép quyền camera.

## Dữ liệu và tài khoản demo

Không có bản backup database đang sử dụng trong hồ sơ. Dữ liệu tái tạo bằng `BusBooking/Data/SeedData.cs`, sau khi áp dụng toàn bộ migration. Xem `Database/README_DATABASE.md`. Không cần dataset bên ngoài để chạy luồng mẫu cơ bản.

Seed tạo role/tài khoản thử, 3 tuyến, 2 mẫu bố trí ghế, 3 xe, chuyến từ ngày mai đến 14 ngày tiếp theo theo giờ Việt Nam và một đơn đặt vé mẫu đã ghi nhận khoản thu mô phỏng. Không phải dữ liệu vận hành doanh nghiệp. Dữ liệu trên máy đang chạy có thể khác bộ seed.

| Vai trò | Email demo |
| --- | --- |
| Admin | admin@busbooking.test |
| Staff | staff@busbooking.test |
| Driver | driver@busbooking.test |
| Customer | customer@busbooking.test |

Mật khẩu seed chung: `BusStudy!2026` — chỉ cho tài khoản thử trên database học tập. Seed không đặt lại mật khẩu tài khoản đã tồn tại. Số tài khoản thanh toán trong cấu hình Development của bản đóng gói được thay bằng số minh họa `000000000000`; chức năng thanh toán là mô phỏng.

## Kiểm tra trước khi nộp

Đăng nhập bằng hai phiên trình duyệt, tìm một chuyến tương lai, chọn cùng ghế để quan sát giữ ghế và cập nhật SignalR; xác nhận vé, kiểm tra Admin/Staff, vé điện tử và check-in. Giữ ghế mặc định 300 giây, tối đa 6 ghế. Các tập lệnh kiểm thử và điều kiện chạy nằm trong `Tests/README.md` và các tài liệu cùng thư mục.

Chỉ kiểm thử tranh chấp/seed trên CSDL riêng, không trỏ vào dữ liệu cần giữ. Để khôi phục kiểm thử JS: `cd Tests`, `npm install`, `npx playwright install chromium` khi cần browser. `package.json` hiện chưa có lockfile; không dùng `npm ci` ở lần đầu.

## Trạng thái bàn giao

Đây là bản sao mã nguồn đã loại thư mục build/cache/cấu hình riêng trên máy. Không thay đổi mã chức năng hoặc CSDL gốc. Tại lần đóng gói 22/11/2026, lệnh build trong phiên tự động bị hủy ở tác vụ compiler (MSB5021); chưa xác nhận build/chạy SQL hoặc demo thành công trong phiên này. Các báo cáo kiểm thử cũ ghi nhận lần chạy trước, không thay thế kiểm tra trên máy người nhận.

Không chạy toàn bộ file SQL theo thứ tự tên. Luồng khuyến nghị là EF migrations; `BusBooking_FullSchema.sql` và các script bổ sung chỉ dùng khi đã đối chiếu schema và `__EFMigrationsHistory`. Xem `SQL_SERVER_SETUP.md`, `Database/README_DATABASE.md`, `LIMITATIONS.md`, `ASSET_SOURCES.md`.

## Đưa lên GitHub

Đưa **nội dung thư mục `01_MaNguon_GitHub`** lên repository; README và solution phải ở gốc repository. Không tải nguyên ZIP thay cho mã nguồn. Sau khi push, điền URL thật ở phần Thông tin nộp bài. Với repository private, mời GVHD tại Settings → Collaborators → Add people và xác nhận lời mời đã được chấp nhận. Hướng dẫn thao tác từng bước nằm trong file Word đi kèm hồ sơ.