# Database changelog — AddContactMessages, 02/11/2026

## Phạm vi

Giữ nguyên Initial 20261029000100_InitialSqlServer, InitialModelV1, bảng/constraint/index nghiệp vụ cũ, Identity và baseline/history hiện có. Migration mới: **20261102000100_AddContactMessages**.

Thêm duy nhất ContactMessages: Id uniqueidentifier PK; SubmissionKey uniqueidentifier unique; FullName nvarchar(100), Email nvarchar(254), Phone nvarchar(25) nullable, Subject nvarchar(160), Body nvarchar(4000), CreatedUtc datetime2, IsRead bit. Thêm IX_ContactMessages_SubmissionKey (unique) và IX_ContactMessages_IsRead_CreatedUtc. Không thêm FK, cột hay trigger vào bảng cũ.

AppDbContext chỉ thêm DbSet và một cấu hình ContactModelV1. Snapshot và BuildTargetModel của migration mới gồm InitialModelV1 + ContactModelV1; target model của Initial không thay đổi. Model V1 của cả hai phần được đóng băng để migration tương lai không làm thay đổi lịch sử.

## Cập nhật máy đã chạy ổn định

1. Dừng web. Trong SSMS dùng tài khoản quản trị database, Tasks → Back Up → Full; giữ file backup ở máy bạn, không đưa vào ZIP.
2. Chọn đúng database mà ứng dụng hiện sử dụng. Không chuyển connection string sang một database mới và không chạy legacy schema.
3. Chạy toàn bộ Database/20261102000100_AddContactMessages.sql. Script yêu cầu Initial đã được ghi nhận, kiểm ContactMessages/history để tránh tạo trùng, dùng transaction và application lock; chỉ khi tạo thành công bảng/index mới ghi history của **migration mới**. Không sửa/xóa bản ghi history cũ. Nếu đã có bảng nhưng chưa có history mới, script dừng để đối chiếu.
4. Trở lại solution, chạy `dotnet run --project BusBooking --launch-profile Https -- --check-db`. Cần Pending migrations: 0.
5. F5, thử biểu mẫu và Admin tiếp nhận. Không cần seed, thay mật khẩu hay chỉnh dữ liệu đang có.

SQL login busbooking_app có db_datareader/db_datawriter sẽ có quyền đọc/ghi cả bảng mới; không cần cấp db_owner cho tài khoản vận hành. DDL được chạy bằng tài khoản quản trị trong SSMS. Nếu tài khoản triển khai hiện tại đã có quyền DDL, có thể dùng `dotnet ef database update --project BusBooking` thay cho script, đúng database hiện tại. Không tự cấp quyền DDL toàn bộ chỉ để tiện thao tác.

## Giới hạn kiểm chứng

Migration C# và SQL companion được viết, đối chiếu và kiểm tra tĩnh; SQL companion không được tuyên bố là output của `dotnet ef migrations script` vì CoreCLR không khởi tạo được ở môi trường bàn giao. Chưa chạy model-diff hoặc thực thi DDL trên SQL Server TEST: CHƯA KIỂM CHỨNG. Chưa tác động database riêng của người dùng.

Up chỉ CreateTable và 2 CreateIndex; Down chỉ DropTable(ContactMessages), sẽ mất các liên hệ của bảng đó nếu chủ động rollback. Quy trình cập nhật không chạy Down, không xóa bảng cũ và không tự migrate/seed tại startup.


## AddTripStops — 03/11/2026

Migration tăng thêm `20261103000100_AddTripStops`; giữ nguyên InitialSqlServer/AddContactMessages và history/baseline. Thêm bảng TripStops (FK Restrict tới Trips, unique TripId/StopOrder, offset >= 0); thêm ID điểm nullable vào SeatHolds/Bookings cùng FK Restrict; Booking có snapshot tên/địa chỉ/giờ UTC/cờ minh họa nullable cho vé cũ. Up không drop bảng/cột cũ; INSERT hai điểm đầu/cuối cho mọi chuyến, không update lịch/giá/xe/ghế/Identity/vé/Payments. Down xóa phần mới và mất lựa chọn điểm: không dùng rollback Down trên DB có dữ liệu mới; backup trước.

`--update-trip-stops [--study-stops]`: idempotent, không seed lại hệ thống, transaction ngắn/trip gate, bỏ qua lịch tùy chỉnh và lịch đã phát sinh hold. Command chỉ tăng Revision/audit metadata khi thêm lịch, không đổi DepartureUtc/ArrivalUtc/Fare/VehicleId/BusRouteId/phân công/trạng thái. Chuyến mới qua CatalogService tự có endpoint trong cùng transaction. Admin sửa lịch chưa có hold qua /Admin/TripStops/id. Xem PICKUP_DROPOFF_GUIDE để cập nhật DB cũ và xử lý baseline, không chạy SQL legacy.

Migration/model diff chưa chạy được bằng EF do lỗi CoreCLR; cần xác minh trên SQL Server test trước khi áp vào DB đang dùng. Không có bản DB riêng của người dùng trong ZIP.
