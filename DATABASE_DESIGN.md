## ContactMessages — phần mở rộng được phép cho Liên hệ

Thêm 1 bảng và 2 index; không đổi bảng nghiệp vụ hay Identity. Thiết kế/độ dài/nullable và hướng dẫn áp dụng tăng thêm nằm ở DATABASE_CHANGELOG.md. Giữ nguyên initial migration và baseline/history đã có. Không chạy SQL legacy chồng lên EF.

---

# Thiết kế dữ liệu

SQL Server rowstore, EF Core 9, Identity. Mọi thời điểm nghiệp vụ UTC (datetime2), hiển thị UTC+7; tiền decimal(18,2). Không có hard-delete giao dịch.

| Thực thể | Khóa / quan hệ | Ý nghĩa |
|---|---|---|
| Routes | PK Id, UQ Origin+Destination | Hành trình, thời lượng dự kiến, hoạt động |
| Layouts | PK Id | Số hàng/cột/tầng; tạo cấu hình ghế |
| PhysicalSeats | PK Id, FK Layout, UQ Layout+Code và Layout+Floor+Row+Column | Ghế vật lý theo cấu hình xe |
| Vehicles | PK Id, UQ Plate, FK Layout | Phương tiện gắn sơ đồ |
| Trips | PK Id, FK Route/Vehicle/Driver/Staff | Một lần khai thác; giá, giờ, sales, trạng thái, revision |
| TripSeats | PK Id, AK Id+TripId, UQ Trip+PhysicalSeat và Trip+Code | Trạng thái độc lập của ghế trên một chuyến; snapshot mã/vị trí |
| SeatHolds | PK GUID, AK Id+TripId, UQ Owner+RequestKey | Chủ sở hữu, hạn 300s, vòng đời, request idempotency |
| HoldSeats | PK HoldId+TripSeatId, FK kép tới hold và ghế theo TripId | Danh sách ghế của hold, giữ lịch sử sau nhả/hết hạn |
| Bookings | PK GUID, UQ Code, UQ SeatHoldId, FK Owner, self FK ReplacesBookingId | Một đơn nhiều hành khách, giá chốt, liên hệ |
| Tickets | PK GUID, FK Booking và TripSeat, unique filtered TripSeatId WHERE IsActive=1 | Một vé cho một hành khách; Trip được suy ra qua TripSeat |
| Payments | PK GUID, FK Booking, UQ Actor+RequestKey | Sổ thu dương/hoàn âm; không chuyển tiền thật |
| ChangeRequests | PK GUID, FK Booking, UQ filtered Booking WHERE Pending | Một yêu cầu đang chờ mỗi đơn; lưu lý do/kết quả |
| AuditEntries | PK bigint | Lịch sử actor/thao tác/đối tượng |
| AspNet* | Identity | Người dùng, role, user-role, claims, login, token |

## Bất biến / constraint

- Trạng thái TripSeat chỉ Available/Held/Booked. Held bắt buộc CurrentHoldId; trạng thái khác phải NULL. FK kép CurrentHoldId+TripId → SeatHold(Id,TripId), ngăn hold của chuyến khác.
- FK kép HoldSeat.TripSeatId+TripId → TripSeat(Id,TripId) và HoldId+TripId → SeatHold(Id,TripId); không thể ghi ghế chuyến khác vào hold.
- Ticket không nhận TripId riêng: không tồn tại cặp TripId/SeatId mâu thuẫn để tin từ client. Confirm đối chiếu chính xác toàn bộ passenger IDs với HoldSeats.
- Unique filtered `IX_Tickets_TripSeatId` với IsActive=1 là lớp bảo vệ cuối khỏi hai vé hiệu lực trên một ghế chuyến. Vé hủy/đổi IsActive=false vẫn giữ lịch sử.
- Check: thời lượng tuyến >0, điểm đi khác đến; hình dạng layout có giới hạn; giá không âm; Arrival>Departure; revision>=0; Expires>Created; khoản Receipt>0/Refund<0.
- Toàn bộ FK Restrict/NO ACTION, tránh cascade xóa vé/tiền/lịch sử. UI dùng IsActive cho danh mục.
- TripSeats phải được tạo cùng transaction khi tạo chuyến, trước khi mở bán.

## Index chính

Trips(RouteId,DepartureUtc,Status,SalesOpen) tìm kiếm; Trips(VehicleId,DepartureUtc,ArrivalUtc), Trips(DriverId,DepartureUtc,ArrivalUtc) xếp lịch. TripSeats(TripId,Status) đọc trạng thái; SeatHolds(Status,ExpiresAtUtc,TripId) quét hết hạn; Bookings(OwnerId,CreatedUtc) lịch sử và (Status,CreatedUtc) báo cáo; Payments(CreatedUtc) tiền ghi sổ. Các FK có index tương ứng.

## Bảo toàn sơ đồ / lịch

TripSeat chụp mã/hàng/cột/tầng từ PhysicalSeat. Sơ đồ đã gắn xe không thay cấu trúc; xe đã có chuyến không đổi layout. Tuyến đã dùng không đổi hành trình/thời lượng. Chuyến có bất kỳ lịch sử hold không đổi xe, tuyến, giờ, giá, chỉ được đổi phân công hợp lệ. Không có nghiệp vụ xóa chuyến/danh mục đã có lịch sử.

CatalogService dùng `sp_getapplock` transaction-owner cho thay đổi danh mục, role và phân công, dùng chung SQL Server giữa các app instances. Kiểm chồng lấn [Departure,Arrival), xét xe/tài xế/nhân viên; buffer cấu hình 0 phút mặc định, không tự nhận là quy định nhà xe. Phân công phải là tài khoản hoạt động đúng role. Người còn được phân công chưa hoàn thành/hủy không bị đổi role/khóa trước khi đổi phân công.

IsActive trên danh mục ngăn chọn cấu hình ngừng dùng khi tạo/sửa chuyến; không tự hủy các chuyến đã công bố. Chuyến đang vận hành được đóng bán/hủy tường minh qua Admin để xử lý hold/vé nguyên tử.


## Bổ sung lịch điểm theo chuyến

TripStops thuộc Trip, có thứ tự và offset UTC theo khởi hành. SeatHold lưu ID cặp điểm để refresh; Booking lưu cặp ID và snapshot tên/địa chỉ/giờ. FK Restrict, cột mới nullable giữ vé cũ. Một booking dùng một cặp điểm cho toàn bộ hành khách. Toàn bộ ghế vẫn bị chiếm theo chuyến, không segment inventory/segment pricing. Lịch đã có hold bị khóa thay đổi qua service; giá chốt/thu/hoàn/Identity không đổi. Chi tiết ở PICKUP_DROPOFF_GUIDE và DATABASE_CHANGELOG.
