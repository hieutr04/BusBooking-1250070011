# Giải thích kỹ thuật khi bảo vệ

- **Vì sao cần Ghế chuyến?** Ghế A01 vật lý trên cùng xe có thể trống ở chuyến sáng, đặt ở chuyến chiều. Trạng thái nằm ở TripSeat, không đặt trực tiếp lên PhysicalSeat.
- **Giữ 5 phút có giữ lock 5 phút không?** Không. Lock chỉ kiểm tra/cập nhật vài bước trong transaction. Quyền giữ là dòng SQL có Owner, GUID và ExpiresAt. Người dùng nhập form sau commit.
- **Nếu hai khách cùng bấm?** Cùng khóa Trip rồi ghế; người đến sau nhìn trạng thái mới sau lock wait. Database có unique filtered ticket index. Phải chứng minh bằng test SQL thật20/50/100, hiện chưa có kết quả.
- **Nếu confirm đúng lúc hết hạn?** UPDATE cuối có `ExpiresAtUtc>SYSUTCDATETIME()` sau các thao tác ghi; bằng hoặc quá hạn không được chuyển. Rollback cả đơn/vé/đổi cũ. UI không quyết định.
- **Refresh có gia hạn?** Không. Snapshot lấy hạn cũ, request key retry trả hold gốc. Token cũ không nhả hay xác nhận ghế của người mới.
- **Worker tắt hoặc app restart?** SQL giữ hạn; API reclaim/check vẫn đúng. Worker chạy lại thu Active quá hạn. Chưa có bằng chứng restart process thực; có testcase cần chạy, không được nói đã test.
- **Vai trò SignalR?** Invalidation sau commit, không sở hữu ghế. Revision chống đảo sự kiện, reconnect/poll snapshot. Không phát token/PII.
- **Doanh thu tính sao?** Receipt dương, Refund âm theo ngày ghi sổ; chưa thu không thành doanh thu. Vé hủy đã thu chờ hoàn, không tự ghi refund khi chưa xử lý.
- **Vì sao không sửa xe/layout tùy ý?** Bảo toàn vé/lịch sử: đã có hold thì khóa cấu hình chuyến, layout đã gắn xe không đổi shape. Tạo cấu hình mới thay vì sửa làm sai vé.
- **Độ trễ đo sao?** Server Stopwatch từ ngay sau commit quan sát đến ACK về server; đây là RTT, không một chiều. Không lấy hai clock khác máy trừ nhau.
- **Đã kiểm thử đạt chưa?** Chỉ đọc đúng TEST_RESULTS; C#/JS syntax không phải build/runtime. Không dùng số0 double booking khi chưa chạy SQL.

Lộ trình trình bày trên máy đã chạy được: ERD → cùng ghế hai chuyến → hai tài khoản tranh ghế → countdown/reload → confirm/vé → Staff thu tiền → yêu cầu đổi/hủy → Driver đúng chuyến → dashboard → raw log test. Không chỉnh sửa source/DB thủ công để giả lập kết quả “đạt”.
