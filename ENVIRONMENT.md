# Môi trường và cấu hình BusBooking

## Môi trường

- Windows; .NET SDK theo global.json: 9.0.305, rollForward latestFeature; target net9.0. Máy đóng gói có SDK 9.0.321 và 10.0.401. Sự hiện diện của SDK không chứng minh build đã thành công.
- ASP.NET Core MVC/Identity/SignalR từ shared framework .NET 9; không yêu cầu package SignalR server riêng.
- SQL Server thực, dùng nhà cung cấp Microsoft.EntityFrameworkCore.SqlServer. Không có fallback SQLite/in-memory. SQL Server 2022 Developer/Express có thể dùng cho môi trường học tập; xác nhận tương thích trên máy chấm.
- Visual Studio có workload ASP.NET and web development là tùy chọn; có thể dùng dotnet CLI.
- Node.js/npm và Chromium chỉ phục vụ bộ kiểm thử JS/browser, không phải điều kiện chạy website MVC.

## Tập tin khai báo thư viện

`BusBooking/BusBooking.csproj` là nguồn chính của package NuGet. `DEPENDENCIES.csv` tổng hợp các khai báo trong dự án. `.config/dotnet-tools.json` ghim dotnet-ef 9.0.9. `Tests/package.json` ghim Playwright 1.56.1 và jsdom 26.1.0. Các thư viện browser được lưu trong `BusBooking/wwwroot/lib`; giữ nguyên thông báo giấy phép đi kèm. Tên và file cụ thể được liệt kê trong `FILES_SHA256.csv`.

Khôi phục: `dotnet restore BusBooking.sln`; `dotnet tool restore`. Kiểm thử JS tùy chọn: vào Tests rồi chạy `npm install`; bộ này chưa có package-lock.json. Thư viện bắc cầu do NuGet/npm giải quyết khi restore/install; danh sách ở CSV là thư viện khai báo trực tiếp, không phải lockfile.

## Cấu hình

- `BusBooking/appsettings.json`: DefaultConnection để trống; Booking, Contact, Logging, AllowedHosts.
- `BusBooking/appsettings.Development.json`: cấu hình development, số tài khoản QR minh họa đã thay trong bản sao đóng gói.
- `BusBooking/Properties/launchSettings.json`: SqlServer tại http://localhost:5190; Https tại https://localhost:7190 và HTTP 5190; ASPNETCORE_ENVIRONMENT=Development.
- `.config/dotnet-tools.json`, `global.json`: công cụ EF và SDK.
- `Documentation/APPSETTINGS_LOCAL_EXAMPLE.json`: mẫu tham khảo, không được ứng dụng tự đọc. Muốn áp dụng thông tin kết nối, dùng User Secrets theo README.
- UserSecretsId: BusBooking-Development-2026. Nếu máy có User Secrets cũ cùng ID, đặt lại đúng connection string cho CSDL thử riêng. Không đưa secrets thật vào hồ sơ.
- Biến môi trường `ConnectionStrings__DefaultConnection` có thể ghi đè User Secrets; kiểm tra khi ứng dụng kết nối nhầm DB.
- HoldSeconds=300; ExpiryScanSeconds=5; ExpiryBatchSize=50; MaxSeats=6. Ràng buộc trong Program.cs yêu cầu HoldSeconds đúng 300.

Thông tin liên hệ trong appsettings là cấu hình sẵn có của dự án; người nộp tự xác nhận tính chính xác trước khi công bố. Không dùng cấu hình payment demo như giao dịch ngân hàng thật.