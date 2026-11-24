# Thuật toán giữ ghế thực tế

## Nguồn quyết định

`Services/BookingService.cs`, `Services/Infrastructure.cs` (DbGate), SQL Server. Không dùng dictionary/lock C#/SignalR connection để phân quyền giữ ghế. RealtimeMetrics có bộ nhớ chẩn đoán, hoàn toàn không tham gia quyền ghế.

## Khóa và thứ tự

Mỗi thao tác dùng transaction ReadCommitted **ngắn**. Khóa Trip theo ID tăng dần bằng `SELECT * FROM dbo.Trips WITH (UPDLOCK, ROWLOCK) WHERE Id=@id`. Đây là gate chung cho tất cả người ghi (web, staff, worker, đóng/hủy chuyến). Sau đó khóa các dòng TripSeat hiện hữu theo ID tăng dần, cùng hints. Đổi vé khóa hai Trip tăng dần trước các ghế. Transaction không kéo dài lúc nhập form/đếm ngược, không gọi SignalR trong transaction.

Thiết kế chủ ý tuần tự hóa các thay đổi trong cùng chuyến để làm rõ tính đúng đắn và tránh đua đóng chuyến với confirm. Chuyến khác độc lập. Có đánh đổi throughput; **chưa có số tải đo được**. ROWLOCK là hint, có thể khóa index key/page/escalate; tính đúng không phụ thuộc tuyên bố “luôn khóa row”, mà dựa vào gate/transaction/constraint. Cần xem execution plan/lock monitoring trên SQL Server thực nếu đánh giá hiệu năng.

## Acquire

1. Xác thực ở controller, giới hạn 1–6 ID ghế phân biệt, RequestKey khác GUID rỗng.
2. Begin transaction → khóa Trip. Nếu (OwnerId,RequestKey) đã tồn tại, kiểm đúng Trip/danh sách, trả lại hold gốc và hạn gốc, không gia hạn (kể cả gốc đã kết thúc).
3. Khóa ghế đúng Trip. Đọc SYSUTCDATETIME sau lock wait. Kiểm Scheduled, SalesOpen, Departure>now.
4. Thu hồi các hold quá hạn đang chạm ghế yêu cầu trong transaction; kiểm đúng CurrentHoldId trước nhả. Không cần chờ worker.
5. Nếu bất kỳ ghế nào không Available: 409 và rollback toàn bộ. Nếu được: GUID mới, Owner, Created=SQL now, Expires=now+300s, HoldSeats và trạng thái Held.
6. Save, Commit. Sau commit mới Publish revision và trả HoldView riêng tư.

## Confirm và mốc hết hạn cuối cùng

Khóa Trip(s), kiểm owner/hold, exact tập ghế và chuyến. Booking.SeatHoldId unique phục vụ idempotency: cùng hold thành công luôn trả lại cùng Booking.Id; nếu retry mang ExchangeBookingId khác, từ chối.

Tạo Booking/Tickets, thay Held→Booked, giá lấy từ Trip trong SQL (không có giá trong DTO). Với đổi vé: kiểm yêu cầu Exchange Pending, khóa ghế cũ, vô hiệu vé cũ/nhả ghế; flush trong cùng transaction để giải phóng filtered unique slot. Chưa commit.

**Sau các insert/index waits**, chạy SQL thực trong `Confirm`:

```sql
UPDATE h SET [Status]=N'Confirmed'
FROM dbo.SeatHolds h JOIN dbo.Trips t ON t.Id=h.TripId
WHERE h.Id=@hold AND h.OwnerId=@actor AND h.[Status]=N'Active'
  AND h.ExpiresAtUtc > SYSUTCDATETIME()
  AND t.[Status]=N'Scheduled' AND t.SalesOpen=1
  AND t.DepartureUtc > SYSUTCDATETIME();
```

Affected rows phải là 1. Nếu hết hạn đúng tại bước chuyển cuối, ném BusinessException và rollback **cả đơn/vé mới lẫn thay đổi vé cũ**. Mốc tuyến tính là UPDATE có điều kiện trước commit; không hứa commit vật lý luôn xong trước deadline. Không chỉ dựa vào `now` đọc trước lúc chờ khóa hoặc countdown.

## Release / expire / đóng chuyến

Release kiểm owner, hold current, không tác động hold khác; Active hết hạn chuyển Expired, còn hạn chuyển Released. Retry Released/Expired no-op. Confirmed từ chối nhả.

Worker dùng PeriodicTimer, interval=5s, batch=50 hold; query có index (Status,ExpiresAtUtc,TripId). Mỗi Trip được xử lý bằng scope/transaction riêng. API Acquire chủ động reclaim đúng hold liên quan dù nó chưa nằm trong batch worker. Worker chỉ lấy Active quá hạn, không đụng Confirmed/Booked; đọc lại SQL clock sau locks, commit rồi phát event. Lỗi được log và vòng quét tiếp theo thử lại. Restart không viết lại hạn; các dòng vẫn ở SQL.

Đóng bán/Boarding nhả Active holds, không hủy vé Booked. Hủy chuyến còn Scheduled/Boarding hủy vé và nhả ghế/hold cùng transaction, không tự tạo refund ledger. Driver chưa đủ quyền hủy chuyến.

## Idempotency / lỗi sau commit

| Thao tác | Khóa logic | Retry |
|---|---|---|
| Hold | Owner+RequestKey unique | Cùng tập ghế trả hold cũ, không gia hạn |
| Confirm/Exchange | HoldId unique trên Booking | Trả Booking.Id đã tạo, giữ nguyên payload đầu thành công |
| Release | HoldId và owner | No-op nếu đã nhả/hết hạn, không nhả hold mới |
| Cancel | BookingId, status | Cancelled no-op; Exchanged từ chối |
| Thu/hoàn | Actor+RequestKey unique | Kiểm cùng Booking và loại khoản, không ghi thêm |
| Yêu cầu đổi/hủy | Unique Pending trên Booking | Cùng loại trả lại; loại khác 409 |

SQL unique/deadlock/lock-timeout dịch thành 409 có hướng dẫn resync. Không bật EF retry tự động bao quanh transaction một cách mơ hồ. Lỗi publish được catch/log sau commit, không nói SQL rollback hoặc trả thất bại giả cho đơn đã commit. Mất HTTP response có thể retry cùng key/token. Cancellation trước commit rollback qua Dispose transaction.

## Căn cứ kỹ thuật

- [Microsoft SQL Server table hints](https://learn.microsoft.com/en-us/sql/t-sql/queries/hints-transact-sql-table)
- [Hosted services, tạo scope cho worker](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-9.0)

Đã tham khảo các trang trên; chưa thay cho kiểm chứng lock/load thực tế.


## Điểm đón/trả không thay quyền ghế

Acquire kiểm/lưu ID điểm trong trip gate, không đổi thuật toán giữ tất cả/không giữ. TTL 300 giây giữ nguyên. Đổi điểm trên hold qua UpdateStops không đổi ExpiresAt, không tạo hold, không chuyển Booked. Confirm kiểm cặp điểm và lưu snapshot trong cùng transaction tạo vé; deadline SQL cuối giao dịch và rollback đổi vé giữ nguyên. Retry confirm không sửa snapshot đã tạo.
