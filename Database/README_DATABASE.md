# Cơ sở dữ liệu và tái tạo dữ liệu mẫu

Hồ sơ cung cấp mã schema/migration, SQL tham khảo và mã seed. Không kèm backup của CSDL đang dùng, vì phiên đóng gói chưa kết nối/xuất SQL Server. Yêu cầu cung cấp dữ liệu hoặc hướng dẫn truy xuất được đáp ứng bằng quy trình tái tạo dữ liệu học tập dưới đây. Nếu GVHD cần đúng dữ liệu demo hiện có trên máy sinh viên, thực hiện phần xuất bổ sung.

## Dựng mới trên máy chấm

1. Tạo CSDL riêng và cấu hình DefaultConnection bằng User Secrets như README. Chạy tại thư mục solution.
2. `dotnet restore BusBooking.sln`
3. `dotnet tool restore`
4. `dotnet ef database update --project BusBooking`
5. `dotnet run --project BusBooking --launch-profile SqlServer -- --check-db`
6. Khi Pending migrations bằng 0: `dotnet run --project BusBooking --launch-profile SqlServer -- --seed`
7. `dotnet run --project BusBooking --launch-profile SqlServer` rồi truy cập http://localhost:5190.

`--seed` chỉ chạy trong Development. Nó tạo dữ liệu qua EF Core, Identity và service nghiệp vụ. Tài khoản mẫu nằm trong README. Seed không đặt lại dữ liệu hoặc mật khẩu hiện có; vẫn có thể bổ sung chuyến khi chạy lại vào ngày khác. Chỉ áp dụng với DB thử riêng.

Không dùng `EnsureCreated`, không chèn giả lịch sử migration, không chạy đồng thời SQL tạo schema và EF trên một CSDL trống. Khi dùng DB đã có, sao lưu và xem migration còn thiếu trước khi update; không drop DB.

## Nguồn dữ liệu

- Cấu trúc: `BusBooking/Data/AppDbContext.cs`, `Data/Migrations/`, các model trong `Models/`.
- Dữ liệu cơ bản: `BusBooking/Data/SeedData.cs`; 3 tuyến, 2 layout, 3 xe, chuyến tương lai trong 14 ngày, tài khoản thử và 1 đơn mẫu.
- Các công cụ mở rộng như HighlandsSchedule/PairedFleetSchedule và script SQL trong thư mục này là tài liệu/mã hiện có; không bắt buộc để chạy mẫu cơ bản. Đọc riêng trước khi áp dụng.
- Ảnh tĩnh: `wwwroot/images/`, nguồn và giấy phép tại `ASSET_SOURCES.md`, `Documentation/PHOTO_ASSETS.json`. Không cần tải dataset ngoài để chạy mẫu cơ bản.

## Xuất đúng dữ liệu demo hiện tại nếu cần

Trên SQL Server chứa dữ liệu demo, mở SSMS → chọn đúng DB → Tasks → Generate Scripts → chọn schema và các bảng cần nộp → Advanced → Types of data to script = Schema and data → lưu thành `BusBooking_Demo_SchemaData.sql`. Kiểm tra dữ liệu tài khoản, hành khách, email, số điện thoại, lịch sử thanh toán trước khi chia sẻ. Chỉ xuất bộ dữ liệu thử đã được phép; không công bố dữ liệu khách hàng hoặc mật khẩu thật.

Cách phục hồi file xuất: người nhận tạo DB trống riêng, chọn đúng DB trong SSMS rồi chạy script đã kiểm tra. Nếu script có đủ schema và `__EFMigrationsHistory`, chạy --check-db để đối chiếu trước khi dùng EF. Không chèn history thủ công khi thiếu. Backup `.bak` là phương án trao riêng nếu GVHD yêu cầu và phiên bản SQL Server tương thích; không bắt buộc cho phương án seed.

## Phân biệt các file SQL

`BusBooking_FullSchema.sql` chứa lịch sử migration, nhưng phải đối chiếu với toàn bộ migration C# hiện tại trước khi sử dụng. `SCHEMA_REVIEW_ONLY.sql` phục vụ đọc thiết kế. Các file có ngày là script thay đổi/bổ sung theo từng giai đoạn; không phải chuỗi bắt buộc chạy mới. Quy trình chính của bộ nộp là EF migrations + seed như trên.