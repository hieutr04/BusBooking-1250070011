# Giới hạn bản UI nâng cấp01/11/2026

Source đã nâng UI và redirect; chưa được restore/build/run với SQL hoặc browser tại môi trường lần sửa này. Không có screenshot thật, không khẳng định hết500/đạtcontrast/viewport. CoreCLR init thất bại dù SDK9.0.305 đã được tải. SQL/concurrency20/50/100,300s thực,role/returnUrlHTTP và hiệu năng đều CHƯA KIỂM CHỨNG. Các báo cáo baseline giữ nguyên số đo chưa có.

Không thay schema,UserSecretsId,config/provider/seed hoặc thuật toán giữ ghế. Layout chỉ preview hình chữ nhật theo field hiện có, chưa lưu; giới hạn danh sách100/200 vẫn giữ. Không thêm phân trang/filter giả. Không có template ảnh/font/JS từ website tham khảo, chỉ học bố cục/UX đã đọc được. Các giới hạn kỹ thuật/nghiệp vụ baseline bên dưới vẫn áp dụng.

---

# Giới hạn và khác biệt cần đối chiếu

1. **Thiếu Word và ảnh nhận xét gốc.** Chỉ prompt Markdown hiện có quyết định scope triển khai; chưa xác nhận phù hợp mọi câu trong đề cương/ảnh. Không sửa Word. Không bịa mục bị cắt hoặc số đo định lượng.
2. **Chưa build/runtime.** Môi trường không khởi tạo được CoreCLR dù đã tải SDK9.0.305; không có SQL Server/Visual Studio. Chưa có bằng chứng migration/model snapshot khớp, Razor compile, route chạy, workflow, bốn role hoặc browser responsive. Những phần này cần chạy trên máy .NET9/SQL Server.
3. Test 20/50/100 HTTP, đủ 300 giây, worker restart process, fault injection và latency SignalR **chưa chạy**. Không có số P95/mean/double booking hay SLA đã đạt. Các source checks không thay thế.
4. Migration initial/snapshot soạn tường minh từ model. Phải `dotnet ef migrations has-pending-model-changes` và kiểm trên DB test; không suppress warnings. Chưa có kết quả chạy DDL.
5. Đổi/hủy là yêu cầu nhân viên xử lý trước khởi hành. Chưa có mức phạt/phí/khấu trừ vì không có căn cứ nguồn; công bố rõ không tự chuyển tiền. Tiền vé cũ chờ hoàn thủ công; vé mới thu riêng, không tự chuyển khoản hay ví.
6. Layout dùng hình chữ nhật 1–20 hàng, 1–4 cột, 1–2 tầng; mã ghế tự sinh. Không có trình kéo/thả sơ đồ tùy ý; các vị trí được quản lý bằng cấu hình này. Có giới hạn bảo toàn layout/xe/tuyến/chuyến đã dùng.
7. Cùng chuyến dùng trip gate tuần tự hóa để chứng minh tính đúng; chưa tối ưu tải và chưa đo throughput. Mặc định 1 app instance, không Redis/backplane/outbox. SQL đúng dù mất SignalR, nhưng có thể mất sự kiện; client phục hồi snapshot/poll15s.
8. Buffer lịch 0 phút là cấu hình học tập công khai; thời gian đến dự kiến do Admin nhập. Không tuyên bố đây là quy định vận tải/pháp luật.
9. Các màn hình list có giới hạn 100/200 dòng mới nhất, chưa phân trang lớn; dashboard lọc tối đa366 ngày. Đây là giới hạn triển khai, không phải nghiệp vụ mới.
10. Chưa có email/SMS, cổng thanh toán, POS, GPS, bảo dưỡng, fuel, AI, forum, marketplace, loyalty/voucher hoặc app mobile; các mục này không thuộc phạm vi yêu cầu.
11. Seed học tập là lựa chọn tường minh Development. Chưa cung cấp triển khai production/hosting; không mang mật khẩu test đi dùng thật.

Không dùng tên VERIFIED cho ZIP; tên REVIEWED chỉ thể hiện đã rà source/đóng gói và có báo cáo giới hạn, không phải chứng nhận vận hành.


## Bản STOPS_SEATS

CoreCLR 0x8007000E/exit 137 chặn toàn bộ tool restore/restore/build/run/EF. Không có SQL Server hoặc screenshot MVC thật, không chứng nhận schema migration/model diff hay HTTP 500 đã hết. Có DOM test với fixture HTTP, không thay kiểm chứng backend. Chỉ có lịch endpoint và ví dụ trung gian học tập; không có địa chỉ vận hành thật/ETA giao thông. Lịch có hold history khóa; một tầng giả định ghế, hai tầng giả định giường theo hai loại seed hiện có. Không định giá hoặc tái bán ghế theo chặng. Xem TEST_RESULTS hiện tại trước các báo cáo cũ.
