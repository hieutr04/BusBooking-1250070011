# Bus Center — kết quả kiểm tra 03/11/2026

Source sửa trực tiếp từ BusGo_UI_CLEANUP_STOPS_REVIEWED(2).zip; không truy cập DB người dùng. Nhãn REVIEWED. Các báo cáo BusGo/stops/images trước đây giữ nguyên và là lịch sử, không thay bằng kết quả lần này.

| Phần | Kết quả thực | Bằng chứng và giới hạn |
|---|---|---|
| C# parsing, JS syntax, CSS, asset, brand/caption/source/contract | PASS kiểm tra tĩnh | Documentation/validation/buscenter-source-review.json; tree-sitter không kiểm type/DI/Razor |
| seats.js + seat-layout.js | PASS15 case DOM | buscenter-seat-ui.json; gồm12 case cũ và3 case retained-only/current-hold/new-default; HTTP fixtures, không SQL/SignalR server |
| stop-editor.js | PASS3 case DOM | buscenter-stop-editor.json; full-editor/locked append/preview endpoint qua ngày; không Razor/SQL |
| contact.js + site.js | PASS6 case DOM | buscenter-contact-dom.json; validation/focus/loading/chặn lặp/pageshow/menu/ảnh fallback; không chứng minh ghi SQL hoặc HTTP thành công |
| Restore/build .NET9 và C# planner/policy tests | BLOCKED — CHƯA KIỂM CHỨNG | SDK9.0.305; CoreCLR không khởi tạo, HRESULT0x8007000E, exit137; buscenter-dotnet-restore/build/planner.log |
| SQL thật / model diff / batch dry-run/apply/idempotency | CHƯA KIỂM CHỨNG | Không có SQL Server test truy cập được; chưa có báo cáo TripId người dùng |
| Contact lưu/duplicate/rate/CSRF/Admin/permission | CHƯA KIỂM CHỨNG runtime | Service/Controller/schema giữ nguyên byte; cần chạy integration thật |
| MVC/Razor/DI/HTTP/không500/role/thu hoàn/đổi vé | CHƯA KIỂM CHỨNG | App chưa khởi động được; không suy PASS từ parsing |
| Concurrency20/50/100 / TTL300s / restart / SignalR | CHƯA KIỂM CHỨNG | Giữ harness gốc; không tạo tỷ lệ thành công/P95 giả |
| Browser/responsive375/390/768/1024/1440/keyboard/overflow/contrast | CHƯA KIỂM CHỨNG | Có CSS và browser harness, DOM không đo bố cục thật |
| Screenshot thật Home desktop/mobile, Contact, Trips/Details | CHƯA CÓ | Không dùng mockup/staticHTML/imagegen để giả app MVC |

## Phần lõi kiểm soát được bằng source

Hash kiểm Models/DbContext/migrations/SQL scripts/config/csproj/sln/Account/Contact/worker/hub/DbGate/ảnh WebP giữ nguyên. Toàn BookingService sau khi bỏ đúng phần point eligibility/snapshot mới khớp byte baseline: không đổi fare, seat locks/constraints, owner/token/idempotency,300s, expiry, confirmSQL hạn cuối, exchange/payment/refund. Select/Resolve cũ nguyên byte; wrapper mới kiểm lựa chọn mới hoặc điểm đã giữ theo đúng vai trò. Đây là bằng chứng source, không phải runtime concurrency.

No-op/idempotent có ý nghĩa: plan sameName/address/offset/flags đang phù hợp trả complete, không INSERT/revision/audit; stale revision bị từ chối. Chưa đo hành vi này trên SQL. Dry-run chưa ghi trong source; file JSON before/after chỉ được sinh từ DB khi người dùng chạy lệnh, không có log giả trong ZIP.

## C# regression mới chưa chạy

StopChoicePolicyTests gọi code sản phẩm: loại địa phương giữa đường kể cả đổi IsIllustrative=false; vẫn cho đúng holder dùng ID cũ; thêm endpoints cụ thể không mutate rows; default đúng đầu/cuối; newAcquire từ chối ID cũ bị thay; không lấy ID hold khác; source/operator/area/flags/same-name conflict; lặp không trùng; exactArrivalUTC; ContactOptions không tạo tel/mailto/javascript link sai. Chạy kèm --planner-only nhưng bị chặn CoreCLR.

SQL harness thêm `Specific_endpoints_preserve_old_holds_bookings_and_change_new_defaults_only`: fixture `_Tests`, holdConfirmed/Active/Expired; dry-run không ghi; bổ sung2endpoints; JSON điểm cũ giữ nguyên; holdId/owner/request/created/expires/status/pointIDs giữ nguyên; repeat complete; audit nguồn; newAcquire từ chối endpoint cũ, holder vẫn confirm cũ, người mới nhận endpoint mới và giá toàn chuyến. Các case trước và20/50/100 HTTP vẫn giữ. Chưa thực thi SQL.

## Chạy tiếp trên máy có .NET9/SQL

Tại thư mục có BusBooking.sln:

```powershell
dotnet tool restore
dotnet restore BusBooking.sln
dotnet build BusBooking.sln
dotnet run --project Tests/BusBooking.RedirectTests
dotnet run --project Tests/BusBooking.SqlTests -- --planner-only
dotnet ef migrations has-pending-model-changes --project BusBooking
```

Bộ integration chỉ DB riêng kết thúc `_Tests`; cấu hình SQL_TEST_CONNECTION theo Tests/README, app test dùng cùngDB, không đổi secrets DB thật để chạy test:

```powershell
dotnet run --project Tests/BusBooking.SqlTests -- --ack-test-db --http http://localhost:5190 --five-minutes
```

DOM/source: npm install ở Tests; npm run seat-ui; npm run stop-editor; npm run contact-dom. Source review cần Python tree-sitter/tree-sitter-c-sharp/tinycss2 và Node: `python Tests/buscenter-review.py --baseline <ZIPBusGođầuvào>`.

Browser thật: Tests/BUSCENTER_TEST_GUIDE.md. Script buscenter-browser.mjs từ chối domain ngoài localhost/fixture ngoài `_Tests`; mở app thật mới chụp15PNG (Home/Contact/Trips ×5viewports), không dựng HTML. Chưa chạy nên không có PNG trong lần bàn giao.
