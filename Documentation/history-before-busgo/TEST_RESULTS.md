# Kết quả kiểm tra STOPS_SEATS — 03/11/2026

Input: QuanLyXeKhach_DatVe_GiuCho_REALTIME_IMAGES_CONTACT_REVIEWED(9).zip. Đã đọc đầy đủ prompt tự tạo trong cuộc chat, source mới và tham khảo hướng dẫn FUTA công khai; ảnh trong chat chỉ làm căn cứ visual, Word/nhận xét gốc không đính kèm lại lượt này. Không gọi kết quả dưới đây là xác minh ứng dụng chạy hoàn chỉnh.

| Kiểm tra | Kết quả | Bằng chứng / phạm vi |
|---|---|---|
| Cú pháp C# (Tree-sitter), JS (Node), CSS parser, JSON/XML/source asset | PASS — tĩnh | validation/stops-review.json, stops-static-source.json. Không thay compile/Razor |
| JS renderer và tương tác hold/điểm/tổng tiền | PASS — 10 tình huống DOM | validation/stops-seat-ui.json, Tests/seat-ui.test.cjs; dùng jsdom + HTTP fixtures, không SQL/server/SignalR thật |
| Giữ nguyên solution/csproj/SDK, migrations cũ, Identity/login, Liên hệ, worker, báo cáo và ảnh | PASS — so sánh byte | validation/stops-review.json. Không chứng minh hồi quy runtime |
| .NET SDK 9.0.305 host inventory | Đã tải, host liệt kê bản SDK/runtime; CoreCLR lỗi | validation/stops-dotnet-0.log, exit 137 |
| Tool restore, NuGet restore, build, EF pending-model check, run | CHƯA KIỂM CHỨNG | Đã thử từng lệnh, đều lỗi CoreCLR 0x8007000E trước code app; stops-runtime-attempts.json |
| EF migration/model SQL Server và áp trên DB cũ | CHƯA KIỂM CHỨNG | Không có SQL Server; phải chạy has-pending-model-changes và test migration trên Windows |
| HTTP MVC, Login/role/ownership/CSRF, Admin, Staff, Driver, Liên hệ | CHƯA KIỂM CHỨNG | App không khởi động được; không có response thành công để ghi PASS |
| Acquire/Confirm/đổi-hủy/thanh toán SQL, 20/50/100 request, 300 giây thật | CHƯA KIỂM CHỨNG | Giữ bộ harness cũ, bổ sung 3 case Stops trong SqlTests; chưa chạy |
| Actual browser console, keyboard, Reduce Motion, 375/390/768/1024/1440px, screenshot | CHƯA KIỂM CHỨNG | CSS đã bổ sung breakpoint, test DOM không đo layout; không dùng mockup làm screenshot |
| HTTP 500 | CHƯA KIỂM CHỨNG | Không tuyên bố hết 500 khi chưa chạy app |

## 10 tình huống JavaScript đã chạy

32 nút giữ ID/code/lối đi; 24 giường chia 12/tầng; UTC+7 qua ngày + giờ có mặt; Acquire xóa selected nhưng total vẫn đúng; đổi điểm gọi API giữ nguyên expiry; sync giữ draft và point; trang mới khôi phục activeHold/total; nhiều hold có ghế/điểm riêng; chuyển lại hold khôi phục draft; HTTP fixture 409 không báo giữ thành công giả.

Lệnh thực tế: `NODE_PATH=<test dependencies> node Tests/seat-ui.test.cjs`. Phụ thuộc test là jsdom 26.1.0; khi chạy trên máy bạn: `cd Tests`, `npm install`, `npm run seat-ui`. Không cần Node để chạy web MVC.

## Kiểm thử cần chạy trên máy có SQL Server

Đọc PICKUP_DROPOFF_GUIDE và Tests/README. Giữ connection string, backup DB. Tool restore → restore/build → migrations list + pending-model check → database update trên DB test → command update điểm hai lần đối chiếu CSV → khởi động SqlServer → SqlTests trên `_Tests` → browser.mjs và kiểm viewport/các vai trò. Không chạy harness test lên DB học tập/người dùng.

Báo cáo cũ được lưu nguyên tại Documentation/validation/PREVIOUS_TEST_RESULTS_IMAGES_CONTACT.md để truy vết; không xem đó là PASS cho phiên bản mới.
