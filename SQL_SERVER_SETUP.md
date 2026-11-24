# Cài và chạy SQL Server

## Chọn instance thực

1. Mở SQL Server Configuration Manager/Services: SQL Server instance phải đang Running. LocalDB dùng `sqllocaldb info`; LocalDB không phải dịch vụ SQLEXPRESS.
2. Mở SSMS, Server type Database Engine, chọn đúng Server name và cơ chế xác thực. Thử Connect trước khi sửa website.
3. Tạo DB riêng `BusBookingStudy` nếu tài khoản chạy migration không có quyền CREATE DATABASE. Môi trường development có thể dùng chủ sở hữu DB; tài khoản vận hành chỉ cấp quyền nghiệp vụ cần thiết. Không đưa thông tin production vào source.

Ví dụ Windows Auth cho SQLEXPRESS (chỉ nếu đúng instance):

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\SQLEXPRESS;Database=BusBookingStudy;Trusted_Connection=True;TrustServerCertificate=True" --project BusBooking
```

Đối với SQL login, thay bằng thông tin thật trong User Secrets/biến môi trường `ConnectionStrings__DefaultConnection`; không commit mật khẩu. `TrustServerCertificate=True` chỉ phục vụ development khi chưa có chứng chỉ tin cậy. Production cần TLS/chứng chỉ phù hợp, không triển khai hosting trong ZIP này.

## Một quy trình schema

```powershell
dotnet tool restore
dotnet ef migrations has-pending-model-changes --project BusBooking
dotnet ef database update --project BusBooking
dotnet run --project BusBooking --launch-profile SqlServer -- --check-db
dotnet run --project BusBooking --launch-profile SqlServer -- --seed
```

Migration `20261029000100_InitialSqlServer` tạo các bảng Identity/nghiệp vụ, FK/check/index và EF quản lý `__EFMigrationsHistory`. Snapshot V1 cố định tại `Data/Migrations/InitialModelV1.cs`; không đổi snapshot V1 khi thêm migration sau. Migration dùng T-SQL tường minh; **chưa chạy kiểm model diff / DDL trên SQL Server trong môi trường bàn giao**. Không tắt `PendingModelChangesWarning`, không giả history, không `EnsureCreated` rồi `Migrate`.

File `Database/SCHEMA_REVIEW_ONLY.sql` là bản đọc DDL của migration, không phải quy trình thứ hai. Nếu cần phương án triển khai bằng script: `dotnet ef migrations script --idempotent --project BusBooking --output deployment.sql`; review rồi áp dụng **thay cho** `database update`, không áp dụng cả hai trên schema không có history hợp lệ.

## Backup và lỗi

- SSMS → database → Tasks → Back Up; thực hiện trước thay đổi migration trên dữ liệu cần giữ. Không gửi backup/DB cá nhân vào ZIP.
- `Login failed`: kiểm Windows account/SQL login, quyền DB, instance, Authentication mode. Không tắt auth.
- `Network-related/instance-specific`: so đúng Server name, service, TCP/port nếu dùng server khác; không tự mở firewall/tunnel trong project.
- `Invalid object name`: có thể thiếu migration hoặc app trỏ nhầm DB. `--check-db` và kiểm tên DB trong SSMS. Không chữa bằng seed fallback.
- `There is already an object...`: dừng, backup và xem schema/history. Không drop DB người dùng.
- `Pending model changes`: chạy `has-pending-model-changes`, rà model/snapshot; sửa trên DB test trước.
- 409 tranh chấp/retry: tải snapshot và dùng lại đúng request key; không sửa trực tiếp TripSeats để “mở khóa”.

## Dữ liệu thử

`--seed` chỉ Development, tường minh; không chạy khi F5 bình thường. Vai trò/tài khoản tạo bằng Identity; mỗi tài khoản một role. Script không INSERT PasswordHash. Thông tin hành khách là chuỗi minh họa, số 0900000000 không đại diện khách hàng thật.
