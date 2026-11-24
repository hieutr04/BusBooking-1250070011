# Ghi chú bản UI nâng cấp

Đọc thêm UI_TEST_GUIDE.md trước. Browser helper đã cập nhật workspace login và390px;SQLharness/thuật toán gốc không đổi. Không dùng testDB hoặc config người khác. Các hướng dẫn dưới đây giữ từbaseline.

# Chạy kiểm thử

## Phân biệt bằng chứng

- source-check.py/Node syntax/C# grammar: chỉ static, không build/runtime.
- SqlTests: service qua scope độc lập, SQL Server thật, kiểm bất biến DB.
- SqlTests --http: HTTP thật, cookies/CSRF độc lập, barrier20/50/100, SQL assertions.
- browser.mjs: browser thật trên app .NET/SQL, viewport/roles/IDOR/CSRF/reconnect, đo RTT.
- Restart process thật: checklist riêng, không thay bằng scope mới.

## A. .NET và model

Tại thư mục solution:

```powershell
dotnet tool restore
dotnet restore BusBooking.sln
dotnet build BusBooking.sln
dotnet ef migrations has-pending-model-changes --project BusBooking
```

Migration/snapshot ban đầu soạn tường minh, chưa so model bằng EF. Không suppress warning hoặc giả history. Chỉ khởi tạo trên DB rỗng riêng lần đầu.

## B. SQL chức năng và concurrency HTTP

Đặt SQL_TEST_CONNECTION tới DB riêng kết thúc `_Tests`. Không drop/reset DB; mỗi run tạo fixture có tên riêng và dữ liệu lịch sử có thể tăng dung lượng.

```powershell
$env:SQL_TEST_CONNECTION="Server=(localdb)\MSSQLLocalDB;Database=BusBooking_Tests;Trusted_Connection=True;TrustServerCertificate=True"
dotnet run --project Tests/BusBooking.SqlTests -- --ack-test-db
```

Cần quyền tạo schema/trigger trong DB test. Test cuối hạn tạo trigger TestDelay_<run> có WAITFOR3s chỉ cho insert đơn mục tiêu rồi xóa trong finally. Đây là fault-injection test, không có delay/trigger này trong sản phẩm. Fixture đưa Expires gần hạn để kiểm nhanh; không thay config HoldSeconds300. Nếu bị kill giữa test, quản trị DB test kiểm/xóa đúng trigger TestDelay_* do run tạo, không chạm DB khác.

Terminal A chạy app trên cùng DB test (nhớ biến môi trường mỗi terminal riêng):

```powershell
$env:ConnectionStrings__DefaultConnection="Server=(localdb)\MSSQLLocalDB;Database=BusBooking_Tests;Trusted_Connection=True;TrustServerCertificate=True"
dotnet ef database update --project BusBooking
dotnet run --project BusBooking --launch-profile SqlServer
```

Terminal B đặt SQL_TEST_CONNECTION rồi:

```powershell
dotnet run --project Tests/BusBooking.SqlTests -- --ack-test-db --http http://localhost:5190 --five-minutes
```

`--five-minutes` chờ302 giây thật trong test, không mở transaction/kết nối SQL trong lúc chờ. Không dùng fixture nhanh làm bằng chứng duy nhất300s. Script verify.ps1 -Database -FiveMinutes chạy build/model và harness; app HTTP phải sẵn trên DB test. Output TestResults/<run>/results.json, concurrency-N.json/csv, five-minutes.json.

## C. Browser và SignalR

Tạo fixture mới (không dùng fixture đã bị các test trước thay đổi):

```powershell
dotnet run --project Tests/BusBooking.SqlTests -- --ack-test-db --fixtures-only
```

Dùng đúng fixtures.json mà harness in; app trỏ cùng DB test. Tại thư mục Tests:

```powershell
npm install
npx playwright install chromium
$env:FIXTURES="C:\duong-dan-that\BusBooking\TestResults\<run>\fixtures.json"
$env:BASE_URL="http://localhost:5190"
node browser.mjs
```

Có hai Customer, Staff, hai Driver (một không được giao), Admin. Kiểm viewport375/768/1024/1440, clientA giữ→clientB cập nhật, reload không gia hạn, offline→snapshot phục hồi, xác nhận vé, Customer/Driver IDOR, role/API/CSRF. Kết quả và mẫu server ACK RTT trong browser-results.json. Không coi là đã bao phủ mọi màn hình/form.

Node/npm chỉ dùng cho test, không dùng để chạy website. node_modules/browser cache không nằm trong ZIP.

## D. Restart process thực — chưa tự động hóa

1. Tắt mọi app/worker khác trỏ cùng DB test. Mở app, login Customer fixture, giữ ghế; ghi HoldId/ExpiresAt từ authenticated snapshot và SQL SeatHolds/TripSeats.
2. Tắt process ngay sau acquire, chờ khoảng10s, mở lại cùng SQL DB. Snapshot phải giữ cùng Id/hạn, không cộng300. Ghi thời điểm tắt/mở, HTTP và SQL evidence.
3. Tắt tiếp trước hạn, chờ qua ExpiresAt thật rồi mở lại. Worker thu hồi: SQL holdExpired, CurrentHold không còn trỏ token cũ, ghếAvailable nếu chưa ai giữ mới. Confirm cũ409.
4. B giữ lại; A release/confirm token cũ; B không mất quyền. Lưu HTTP/log/SQL thực tế.
5. Chạy riêng một hold300s bằng --five-minutes. Không lấy test scope mới hoặc sửa fixture ExpiresAt làm bằng chứng restart process.

## E. Checklist thủ công bổ sung

- Tìm chuyến/ngày sai/no-results/đã chạy/đóng bán/không tồn tại; status400/404/409 rõ.
- CRUD tuyến/layout/xe/chuyến; khóa cấu hình đã dùng; lịch trùng xe/tài xế/nhân viên; role người đang phân công.
- Seed2 lần không trùng hoặc reset mật khẩu; có lịch tương lai khi seed lại.
- Sai token/owner/token cũ, nhiều ghế rollback, retry sau mất HTTPresponse.
- HTTP20 có Staff/Customer chung đường bán; thêm đổi ghế bị giữ, hủy/đổi/close/cancel đồng thời.
- Driver đổi URL sang chuyến khác; Customer đổi Guid vé; anti-forgery thiếu/sai; XSS phải encode.
- Dashboard: thu150000→hủy (chờ hoàn150000, thu ròng chưa đổi)→hoàn150000 (thu ròng giảm); đổi vé không cộng tiền2 lần do retry. Lọc ngày VN.
- SQL tắt/schema thiếu/quyền thiếu: không fallback hay200 giả; có mã yêu cầu/log.
- SignalR chậm/ngắt/đảo event; HTTP vẫn bảo vệ DB; reconnectsnapshot; ảnh thiếu; keyboard/reduced motion/tables ở375px.

Ghi PASS/FAIL/CHƯA KIỂM CHỨNG và giữ raw logs từng lần. Không điền trước kết quả.


## Kiểm thử phần Stops/Seats mới

`npm install` tại Tests → `npm run seat-ui`: 10 case DOM/HTTP fixture, không cần SQL, không xác nhận backend hoặc CSS responsive. Source seat-ui.test.cjs chạy JS thật; jsdom chỉ dùng cho test, không load trong app.

SqlTests thêm Stops_validate_persist_restore_snapshot_and_retry; Stops_endpoint_update_is_idempotent_and_preserves_trip_geometry; Stops_UTC_cross_midnight_and_reverse_teaching_template. Cần DB riêng `_Tests`, harness migrate fixture rồi chạy, không dùng DB người dùng. Các case cũ và 20/50/100 HTTP tiếp tục giữ. Browser.mjs thêm assert checkout/total/điểm/snapshot sau reload và chi tiết vé. Các test SQL/browser mới chưa chạy trong môi trường bàn giao.


## BusGo UI cleanup / append-only — 03/11/2026

Xem TEST_RESULTS.md và Tests/BUSGO_TEST_GUIDE.md cho trạng thái hiện tại. `dotnet run --project Tests/BusBooking.SqlTests -- --planner-only` chạy pure production planner không SQL. Bộ SQL có hai case mới append khi có lịch sử và hai lần complete đồng thời; chưa chạy do CoreCLR/SQL unavailable ở môi trường bàn giao. DOM: `npm run seat-ui` (12 case) và `npm run stop-editor` (2 case) đã chạy trong jsdom, không xác nhận MVC/SQL/browser/CSS responsive. Giữ mọi bộ test concurrency/ownership/expiry/đổi/thu hoàn trước đây.


## Bus Center — 03/11/2026

Đọc TEST_RESULTS.md và Tests/BUSCENTER_TEST_GUIDE.md. Current source review là buscenter-review.py; busgo-review.py/báo cáo BusGo là baseline trước thay đổi point-eligibility lần này, không dùng để tuyên bố source cũ hoàn toàn bất biến. DOM hiện tại15+3+6 case, SQL/HTTP/browser chưa kiểm chứng. npm run buscenter-browser chỉ dùng app MVC thật/fixture _Tests; không tạo screenshot giả.
