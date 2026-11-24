# Đánh giá định lượng — CHƯA CÓ SỐ ĐO

| Tải | Mean HTTP ms | P95 HTTP ms | Thành công kỹ thuật | Conflict nghiệp vụ | Số mẫu |
|---|---|---|---|---|---|
| 20 concurrent | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa có |
| 50 concurrent | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa có |
| 100 concurrent | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa có |

HTTP dùng Stopwatch monotonic từ trước PostAsJsonAsync đến đọc xong body. Mọi request trong đợt có mẫu/status riêng. Mean=sum(t)/N; P95 nearest rank=phần tử ceil(.95*N)-1 sau sort (index0). Raw CSV/JSON giữ từng mẫu, đơn vịms.

Technical success rate=(200 thắng hợp lệ+409 conflict hợp lệ)/requests gửi. Một thắng trên100 không tự là99% lỗi kỹ thuật. Payload sai/CSRF/auth/5xx/network không tính conflict. Phải xem body409 và DB state, không chỉ HTTP tổng.

Harness lưu ngày UTC/runtime/OS/ProcessorCount/@@VERSION. Người chạy bổ sung CPU/RAM/SQL edition, số app instances, hash source và mạng vào báo cáo thực tế. Hiện không có cấu hình máy chạy SQL; không bịa SLA/P95.

SignalR đo server Stopwatch từ Publish ngay sau CommitAsync trả về tới ACK trở lại server. Browser thu `postCommitObservedToAckMs`. Đây là **RTT quan sát sau commit**, không một chiều SQL commit→client; bao gồm scheduling/delivery/ACK, mốc bắt đầu hơi sau commit trả về. Không trừ clock hai máy. Hiện chưa có số mẫu/mean/P95. Chi tiết REALTIME_DESIGN.

`Tests/read-load.js` k6 stages20/50/100 đọc snapshot chỉ baseline đọc, không thay test HTTP write tranh ghế+SQL assertions. Điều kiện offline/throttle/mất sự kiện và giới hạn phải ghi theo lần đo thực.
