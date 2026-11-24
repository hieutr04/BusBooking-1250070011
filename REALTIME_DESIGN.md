# Đồng bộ SignalR

`Hubs/SeatsHub.cs`, `RealtimePublisher`, `RealtimeMetrics`, `wwwroot/js/seats.js`. SignalR JS 9.0.6 ở local. Hub công khai chỉ đọc trạng thái chuyến; không có phương thức bán/giữ/nhả ghế hay chat.

- JoinTrip kiểm TripId có thật; một connection một nhóm hiện tại `trip:<id>`, chuyển nhóm bỏ nhóm cũ. Thông tin hành khách không ở Hub.
- Tất cả mutation gọi Publish **sau** CommitAsync. Event `TripChanged`: TripId, Revision tăng dưới SQL trip gate, CommittedObservedUtc. Đây là invalidation tối thiểu; trạng thái ghế luôn lấy từ HTTP snapshot có cùng revision.
- Event không chứa HoldId/token, UserId, PII. Public GET snapshot gồm mã/vị trí/trạng thái, không có người giữ. Khi đăng nhập, MyHolds chỉ của người gọi; response no-store.
- Snapshot lấy trip gate trong read transaction ngắn để revision và ghế nhất quán. Ghế có hold quá hạn có thể hiển thị Available theo SQL now trước khi worker chạy; Acquire vẫn thu hồi nguyên tử.
- JS bỏ event revision <= revision đã áp dụng. Một pipeline snapshot tránh request response cũ ghi đè snapshot mới; nếu event mới đến trong lúc fetch, chạy lại cho tới revision mong đợi.
- Reconnect: Join lại rồi snapshot. Visibility/online: snapshot. Poll 15s là đường phục hồi khi thiếu event; event cũ không làm UI đi lùi. Thay đổi hiệu lực do thời gian được tải lại ngay countdown về 0.
- Countdown lấy ServerUtc + elapsed monotonic `performance.now`, ước lượng midpoint RTT khi nhận snapshot; UI không dùng clock máy khách để quyết định ownership.
- Post-commit publish thất bại: log, DB giữ kết quả, client có thể poll/refresh. Không có outbox/durable event delivery trong bản học tập này; không hứa không mất event.

## Đo thời gian trung thực

Ngay khi vào Publish (sau CommitAsync), server ghi Stopwatch timestamp với (Trip,Revision), lưu tối đa khoảng 1024 mẫu trong 60s. Client nhận event gửi AckRevision về Hub. Chỉ connection đã tham gia đúng nhóm được ACK. Server trừ hai Stopwatch ticks cùng process, log `PostCommitObservedToAckMs`; browser test thu cùng số vào JSON.

Đây là **RTT quan sát sau commit → phát event → client nhận → ACK về server**, không phải một chiều SQL commit→client, không lấy timestamp hai máy trừ nhau. Bao gồm scheduling/delivery/ACK overhead; mốc bắt đầu hơi sau CommitAsync trả về. Chưa có mẫu đo thật. Chẩn đoán chỉ hỗ trợ một app instance; nhiều instance cần message backplane và thiết kế đo riêng (ngoài phạm vi hạ tầng đã chọn).

[SignalR JavaScript client chính thức](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client?view=aspnetcore-9.0).


## Dữ liệu điểm

SeatSnapshot thêm Stops và ArriveEarlyMinutes; HoldView riêng tư thêm PickupStopId/DropoffStopId. Chỉ REST riêng tư cập nhật điểm hold; không broadcast lựa chọn cá nhân hoặc hành khách. TripChanged và ACK/metrics sau commit ghế giữ nguyên. Poll/snapshot khôi phục lựa chọn, input draft trong cùng hold không bị dựng lại sau mọi event.
