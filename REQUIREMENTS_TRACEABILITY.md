## Traceability bổ sung — Ảnh & Liên hệ

| Yêu cầu | Source/bằng chứng | Trạng thái kiểm chứng |
|---|---|---|
| 6 ảnh thật, local, có nguồn/quyền | PHOTO_ASSETS.json, ASSET_SOURCES, các file WebP | Kiểm file/metadata/nguồn đã làm; UI runtime chưa thử |
| Ảnh theo địa danh, fallback trung tính | PhotoCatalog, _Photo, site.js | Rà tĩnh; browser chưa thử |
| Form lưu SQL, PRG, replay | ContactController/Service/Tokens, ContactModelV1 | Rà tĩnh; SQL/HTTP chưa thử |
| Chỉ Admin đọc và mark-read | AdminContactsController và 2 views | Rà authorization/encode tĩnh; HTTP chưa thử |
| Chỉ migration tăng thêm | AddContactMessages.cs + SQL companion | Rà scope/cú pháp; DDL/model diff chưa thử |
| Giữ login/hold/realtime/tiền/quyền | So byte với ZIP đầu vào bằng images-contact-check.py | Kiểm file được giữ nguyên; không thay thế test runtime |

Yêu cầu mới được lưu nguyên văn trong Documentation/INPUT_IMAGES_CONTACT_REQUIREMENTS_ORIGINAL.md. Hai tài liệu yêu cầu gốc được giữ nguyên. Không có Word/ảnh nhận xét giảng viên để đối chiếu trực tiếp lần này.

---

# Truy vết yêu cầu → triển khai → kiểm thử

Nguồn đã đọc: prompt đính kèm (21 phần), nguyên văn ở Documentation/INPUT_REQUIREMENTS_ORIGINAL.md. Word `1250070011_TranTrungHieu_12TMDT_DeCuongDoAnChuyenNganh.docx` và ảnh nhận xét không có trong attachments của lượt làm này. Không gán câu/mục ở đây là đã đọc từ Word/ảnh. Các điểm giảng viên được nhắc từ prompt, còn chờ đối chiếu nguồn thật.

**Có source không đồng nghĩa PASS runtime.** “Chưa” dưới đây là CHƯA KIỂM CHỨNG trên .NET/SQL/browser.

| ID / yêu cầu prompt | Module / file / API | Test / bằng chứng dự kiến | Kết quả hiện tại |
|---|---|---|---|
| 1–2 Tên đề tài, ZIP source mới | README, Layout footer, BusBooking.sln | File/scope scan | Có source; chưa đối chiếu Word/ảnh |
| 3 Phạm vi xe khách, không backend giả | Entities, Services, csproj, Program | Provider/package scan | Rà source; runtime chưa |
| 4 .NET9/EF9/Identity/SQL/local libs | csproj/global.json/Program/wwwroot/lib | --info/restore/build | CoreCLR chặn |
| 5 Solution MVC/Razor, service riêng | Controllers/ViewModels/Services/Hubs/BackgroundServices | Build + HTTP/browser | Chưa |
| 6 Tuyến–Xe–Layout–PhysicalSeat–Trip–TripSeat–Ticket | Entities/AppDbContext/Migrations/ERD | Independent_trip_seats, Database_filtered_unique_ticket_constraint | Có schema/test; SQL chưa |
| 6 Giá server/UTC/check/index/history | Confirm, BusinessTime, DDL | Mismatched_trip_and_passenger_seats | Rà source; SQL chưa |
| 6 Khóa cấu hình, chống trùng lịch | CatalogService | Vehicle_schedule_and_configuration_guards | Chưa |
| 7 Available/Held/Booked, all-or-nothing | Acquire/Confirm/Cancel | Multi_seat_all_or_nothing, Hold_confirm... | Chưa |
| 7 Booked không theo hạn hold cũ | Worker xử lý Active, không Confirmed | Hold_confirm_idempotency_and_no_expiry_of_booked | Chưa |
| 8 Hold300s bền vững/owner/token | SeatHold/HoldSeat/Acquire/Release | Five_minute_configuration_and_retry_no_extension | Chưa |
| 8 Transaction ngắn UPDLOCK/ROWLOCK | DbGate, Confirm SQL UPDATE hạn cuối | SQL concurrency, Final_SQL_deadline_exchange_rollback_after_insert_wait | Chưa |
| 8 Token cũ/idempotency | Service + unique keys | Expiry_ownership_stale_tokens..., Cancel_payment_refund_idempotency | Chưa |
| 9 Worker batch/restart | HoldExpiryWorker, ExpireUnderGate | Worker race; Tests/README restart process | Chưa; scope mới không thay process restart |
| 9 Hủy/đổi nguyên tử | Cancel/Confirm(exchange)/SetTripState | Exchange_rollback_and_success..., Final_SQL_deadline... | Chưa |
| 10 SignalR sau commit/snapshot/revision | Publisher/Hub/seats.js | browser.mjs, raw ACK metrics | Rà source; UI/network chưa |
| 11 Identity/RBAC/CSRF/IDOR | Authorize/ownership/antiforgery | Browser role/API/IDOR, Driver_ownership... | Chưa |
| 12 Public/Customer pages | Home/Trips/Bookings/Account | Browser search→hold→confirm/history | Chưa |
| 12 Admin modules | AdminController/Catalog/Report | CRUD/role/forms/checklist | Chưa |
| 12 Staff bán/thu/hoàn/đổi/trả | Staff + shared BookingService | HTTP20 có Staff; cancel/exchange/payment | Chưa |
| 12 Driver/phân công/hành khách | DriverController/SetTripState | Driver_ownership_and_progression, browser IDOR | Chưa |
| 13 Dashboard từ ledger thật | ReportService/Admin/Chart.js | Đối soát thủ công + service tests | Chưa; không KPI hard-code |
| 14 UI/local/assets/responsive | CSS/JS/SVG/ASSET_SOURCES | Browser375/768/1024/1440 | Source/syntax; browser chưa |
| 15 SQL migration/seed/config | Factory/Migrations/Seed/Scripts | has-pending-model-changes/update/seed2 lần | Chưa |
| 16 Form/status/lỗi/300s thực | ExceptionFilter/global handler | SQL harness --five-minutes, browser | Chưa |
| 17 20/50/100 concurrent HTTP | SqlTests --http | Cookies/CSRF/barrier + SQL assertions | Chưa cả ba |
| 18 Mean/P95/raw/SignalR | Harness timer/RealtimeMetrics/browser | Nearest-rank P95, server postcommit→ACK RTT | Chưa có số đo |
| 19 Tài liệu gắn yêu cầu | Danh mục Markdown | File presence scan | Có; nguồn Word/ảnh chưa |
| 20 Kiểm định thực tế | TEST_RESULTS/validation | Build/SQL/browser | Source review; runtime bị chặn |
| 21 ZIP sạch, không VERIFIED giả | Manifest/packaging check | CRC/exclusions | Xem packaging.json |

## Quyết định triển khai công khai

- Một MVC app, một SQL Server, một role/user. Trip gate ngắn chung mọi kênh. Chưa benchmark throughput.
- Tối đa6 ghế/lượt, all-or-nothing. Layout hình chữ nhật tối đa2 tầng, mã ghế tự sinh. Chưa khẳng định Word có yêu cầu sơ đồ khác hay không.
- Đổi/hủy do nhân viên xử lý theo yêu cầu trước khởi hành. Không tự áp phí/phạt khi chưa có căn cứ; chính sách ở Home/Policy. Tiền vé cũ chờ hoàn trực tiếp, vé mới thu riêng.
- Admin nhập giờ đến dự kiến; buffer0 phút cấu hình, không giả là quy định nhà xe.
- Event invalidation+revision, snapshot/poll15s phục hồi. Không chat/Redis/outbox.
- .NET8→9 đã được người dùng chấp thuận trong prompt; không chuyển10, không sửa Word.


## UI upgrade01/11/2026

Yêu cầu nâng UI gốc ở Documentation/INPUT_UI_REQUIREMENTS_ORIGINAL.md. Đối chiếu từng view/action/role ở UI_PAGE_AUDIT.md; server redirect ở LOGIN_REDIRECT_FIX.md. Models/schema/migrations/hold service giữ nguyên hash (core-preservation.json); không chạy SQL trong môi trường này. Không nâng trạng thái concurrency/performance thành PASS.


## Yêu cầu STOPS_SEATS

| Yêu cầu | Source / kiểm thử |
|---|---|
| Điểm đón/trả + UTC+7 + dữ liệu cũ | TripStopService/Maintenance, AddTripStops, Views/Admin/TripStops, SqlTests case Stops_* (chưa chạy SQL) |
| CTA Đặt vé, giữ 300 giây, summary đúng | Details.cshtml, seats.js, DOM test 10 case đã chạy |
| Layout 32 ghế / 24 giường không đổi ID | seat-layout.js, booking.css, Admin preview, DOM count/ID checks |
| Snapshot vé, quyền và idempotency | BookingService, _BookingStops, Passengers, API stops; SQL/browser hồi quy chưa chạy |
| Đóng gói / hạn chế môi trường | FILE_CHANGES, TEST_RESULTS và validation log |
