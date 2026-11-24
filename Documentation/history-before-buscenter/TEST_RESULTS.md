# Kiểm chứng bản BusGo_UI_CLEANUP_STOPS_REVIEWED

Ngày 03/11/2026. Mã nguồn sửa từ ZIP STOPS_SEATS đính kèm; chưa truy cập DB máy người dùng. Báo cáo bản trước giữ ở Documentation/history-before-busgo và các tệp validation/baseline gốc; không lấy số liệu cũ làm PASS bản này.

| Nhóm kiểm tra lần này | Kết quả | Bằng chứng/giới hạn |
|---|---|---|
| C# source parsing / JS syntax / CSS / asset / giữ nguyên hợp đồng bảo vệ | PASS kiểm tra tĩnh | validation/busgo-source-review.json; tree-sitter không biên dịch type/Razor |
| Production seats.js + seat-layout.js | PASS 12 case DOM | busgo-seat-ui.json; jsdom + HTTP fixtures, không có SQL/SignalR server |
| Production stop-editor.js | PASS 2 case DOM | busgo-stop-editor.json; nhánh thêm riêng còn hoạt động khi full editor khóa; UTC+7 qua ngày |
| .NET restore/build và C# planner tests | BLOCKED — CHƯA KIỂM CHỨNG | SDK9.0.305 tải chính thức; Failed to create CoreCLR, HRESULT0x8007000E, exit137; raw logs busgo-dotnet-*.log |
| EF model diff / migrations / SQL data append / idempotency thực | CHƯA KIỂM CHỨNG | Không có SQL Server test truy cập được trong môi trường này |
| MVC/Razor/DI/HTTP tất cả role, thu/hoàn/đổi vé, không500 | CHƯA KIỂM CHỨNG | Chưa khởi động được app; không suy PASS từ source syntax |
| 20/50/100 client tranh ghế, hold300s, restart, SignalR | CHƯA KIỂM CHỨNG | Giữ harness SQL/HTTP cũ; không đo P95/tỷ lệ giả |
| Browser/responsive375/390/768/1024/1440, keyboard/menu/focus/reduced motion | CHƯA KIỂM CHỨNG | CSS có breakpoint; DOM không đo bố cục/overflow/contrast thực |
| Screenshot website thực | CHƯA CÓ | Không dùng mockup, HTML fixture hoặc ảnh tạo để thay bằng chứng MVC |

DOM đã kiểm:32 ghế/24 giường hai tầng đúng ID, chọn/giữ/tổng tiền/checkout, point selection từ API, refresh/nhiều hold không kéo dài hạn, UTC+7 qua ngày, lỗi409 không dựng hold thành công. Thêm case tên hiển thị bỏ đúng marker, địa chỉ trống ẩn và bỏ aria-describedby, địa chỉ có dữ liệu hiện/aria đúng, thời lượng và giờ có mặt động. Harness không chứng minh backend/giá trị SQL thật.

## Regression đã thêm để chạy trên máy có runtime

C# `StopAppendPlannerTests` chạy code planner sản phẩm: đúng chiều/alias,90/180, không đổi đối tượng điểm cũ, giữ điểm riêng, lặp không trùng, xung đột tên/offset/gap chặn toàn bộ, quá giờ/Boarding/durationkhác, foreignTripId/thứ tự sai, tạo lịch rỗng và UTCquađêm. **Chưa thực thi do CoreCLR**.

SQL harness thêm: tạo chuyến Đà Lạt420 phút mới nhận đủ4 điểm; thu gọn chỉ endpoint trước khi giữ để giả lập lịch cũ; xem trước không ghi; hai lệnh CompleteDalat đồng thời chỉ một lần INSERT/revision; giữ endpoint/hold/expiry và confirmgiátoànchuyến. Case khác có holdActive/Expired/Confirmed kiểm JSON trước/sau của hold/ghế/booking/ticket, điểm cũ, geometry, audit, lặp không trùng và fullSave vẫn bị khóa. **Chưa thực thi SQL**; harness từ chối DB không kết thúc `_Tests`.

## Chạy tiếp trên máy .NET9 + SQL Server

Tại thư mục có solution, giữ cấu hình SQL thật của máy bạn:

```powershell
dotnet tool restore
dotnet restore BusBooking.sln
dotnet build BusBooking.sln
dotnet run --project Tests/BusBooking.RedirectTests
dotnet run --project Tests/BusBooking.SqlTests -- --planner-only
dotnet ef migrations has-pending-model-changes --project BusBooking
```

Chỉ chạy SQL harness trên database riêng `_Tests`, cấu hình SQL_TEST_CONNECTION theo Tests/README; không đổi User Secrets ứng dụng thật để phục vụ test. Chạy app test trỏ cùngDB, rồi:

```powershell
dotnet run --project Tests/BusBooking.SqlTests -- --ack-test-db --http http://localhost:5190 --five-minutes
```

Tests/README và UI_TEST_GUIDE có bước fixture/4role/CSRF/browser; thêm checklist Tests/BUSGO_TEST_GUIDE.md. Chỉ đổi REVIEWED→VERIFIED sau khi build, SQL, HTTP và luồng lõi đạt thực; không xóa lịch sử để đạt test.
