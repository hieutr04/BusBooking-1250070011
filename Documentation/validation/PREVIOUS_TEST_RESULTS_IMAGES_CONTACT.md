## Bản Ảnh & Liên hệ — 02/11/2026 (kết quả mới)

Các kết quả lần trước phía dưới chỉ là lịch sử, không được dùng làm bằng chứng runtime cho bản này.

| Nhóm | Trạng thái | Bằng chứng/giới hạn |
|---|---|---|
| File ảnh thật/local/metadata/license | PASS (kiểm file & nguồn) | 6 WebP đã tải/giải mã, so hash, mở contact sheet, nguồn từng ảnh trong ASSET_SOURCES |
| JSON/csproj/asset paths/JS (63 kiểm tra) | PASS (tĩnh) | TestResults/IMAGES_CONTACT_STATIC.json, IMAGES_CONTACT_CHECK.json |
| Nâng cấp/bảo toàn source (117 kiểm tra) | PASS (so byte/tĩnh) | IMAGES_CONTACT_CHECK.json; không chứng minh runtime |
| C# grammar | PASS (cú pháp) | 40 file qua tree-sitter C#; không phải compile hoặc Razor |
| SQL companion | PASS (cú pháp/phạm vi tĩnh) | Một bảng mới, 2 chỉ mục; parser T-SQL; chưa execute |
| CSS mới | PASS (cú pháp) | Parser CSS; không thay thế browser/layout |
| SDK/restore/build/Razor/DI/model diff | CHƯA KIỂM CHỨNG | Có SDK 9.0.305; --info/--version không khởi tạo được CoreCLR 0x8007000E, exit 137. Restore/build không thể thực thi thành công |
| SQL DDL và Contact lưu/replay/PRG | CHƯA KIỂM CHỨNG | Không có SQL Server/sqlcmd/Docker; chưa áp dụng lên database người dùng |
| HTTP, login/RBAC/anti-forgery/DB failure | CHƯA KIỂM CHỨNG | Có test mới và test cũ; chưa có app runtime để chạy |
| Browser/5 viewport/offline/fallback/keyboard | CHƯA KIỂM CHỨNG | Playwright có package nhưng thiếu executable; cài Chromium thất bại vì payload ZIP hỏng/0 MiB |
| Luồng giữ chỗ/đặt/đổi/hủy/thu-hoàn | CHƯA KIỂM CHỨNG | Không sửa backend; các test cũ giữ nguyên để chạy trên SQL TEST |
| Concurrency 20/50/100, hold 300s, P95/SignalR | CHƯA KIỂM CHỨNG | Không tạo số đo mới hay gán số đo cũ thành kết quả mới |
| Screenshot runtime mới | CHƯA KIỂM CHỨNG | Không có ảnh Home/Trips/Contact/Admin từ app thực trong môi trường này; không bàn giao mockup thay thế |

Lệnh đã thử ở workspace: `.ui-tools/dotnet/dotnet --info`, `--version` với TMPDIR hợp lệ và cấu hình GC giới hạn đều exit 137/CoreCLR; Node Playwright launch thiếu browser và lệnh install chromium --only-shell lỗi ZIP. Chi tiết test tĩnh có JSON kèm theo. Lệnh phục hồi môi trường/EF/app trên máy người dùng nằm trong Tests/CONTACT_TEST_GUIDE.md, chưa được tuyên bố chạy thành công ở đây.

---

# Kiểm thử bản UI nâng cấp — 01/11/2026, giờ Việt Nam

**REVIEWED, CHƯA VERIFIED. Không có screenshot thật của bản nâng cấp trong lần làm này.**

## Môi trường / baseline

Đầu vào (2).zip có116 tệp và31 Razor. Sau sửa có34 Razor. Baseline kiểm bằng source/hash, không xem ảnh người dùng hoặc README cũ là bằng chứng build/run. Người dùng trước đây đã kết nối SQL và chạy bản cũ trên máy Windows; điều đó không chứng minh bản nâng cấp đã chạy trong môi trường này.

Môi trường hiện tại: Linux sandbox, có Node/Python, không dotnet/docker/sqlcmd ban đầu, không SQL Server/Visual Studio. Tải SDK9.0.305/runtime9.0.9 để thử; host không tạo được CoreCLR, HRESULT0x8007000E/exit137. Không đổi framework/provider hoặc dùng backend giả.

## Bảng kết quả

| Nhóm | Expected | Actual / evidence | Kết quả |
|---|---|---|---|
| File config/project/solution/assets | JSON/XML hợp lệ,3 project net9, local assets có thật | ui-static-checks.json; tổng82 kiểm tra source, không phát hiện lỗi | PASS, chỉ static |
| Bảo toàn core | Models/Data/migrations/seed/Program/service/worker/Hub/SeatsApi/controller khác Account vàseats.js không đổi | So SHA256 vớiZIP gốc; ui-static-checks.json vàcore-preservation.json | PASS, chỉ hash |
| Contract sơ đồ ghế | Giữ toàn bộID/data-trip/auth/exchange | So view cũ/mới; seats.js giữ nguyên hash | PASS, chỉ source |
| Auth Register/Logout/Profile | Không thay code ngoài Login/using | So đoạn source chính xác | PASS, chỉ source |
| C# syntax | Không parser errors | ui-syntax.json,29 C# files; không type-check/Razor | PASS, chỉ grammar |
| JS syntax | Node --check không lỗi | site/seats/dashboard/admin và3 browser scripts trong static log | PASS, chỉ cú pháp |
| CSS syntax | Không top-level parse errors | site/ui/admin parse bằngtinycss2 | PASS, chỉ parser |
| Contrast8 mẫu CSS | >=4.5:1 cho các cặp chữ đã chọn | contrast-samples.json,4.89–13.24:1 | PASS cho8 cặp, không toàn trang |
| dotnet --info | SDK/runtime khởi tạo được | exit137/CoreCLR lỗi | CHƯA KIỂM CHỨNG, môi trường chặn |
| dotnet tool restore | EF tool restore | exit137 trước tool | CHƯA KIỂM CHỨNG |
| dotnet restore/build solution | Restore/build C#/Razor/DI | exit137 trước MSBuild | CHƯA KIỂM CHỨNG |
| EF has-pending-model-changes | Không đổi model | Không chạy được EF; hash schema không đổi | CHƯA KIỂM CHỨNG runtime |
| RedirectTests | Helper thuần pass mọi case | Không chạy được .NET | CHƯA KIỂM CHỨNG |
| dotnet run SqlServer | App chạy trên SQL hợp lệ | CoreCLR lỗi; không SQL sẵn | CHƯA KIỂM CHỨNG |
| SQL/seed/HTTP/role/CSRF/IDOR | Hành vi đúng như test cases | Không tạo/kết nối/mutate DB người dùng hoặc test DB | CHƯA KIỂM CHỨNG |
| UI browser/console/network/screenshots | Tương tác thật,5 viewport | App không khởi động; không screenshot/mockup giả | CHƯA KIỂM CHỨNG |
| 300s/expiry/reconnect/concurrency20/50/100 | Invariants đúng trên SQL | Bộ SQL tests gốc giữ nguyên, chưa chạy | CHƯA KIỂM CHỨNG |
| Mean/P95/ACK RTT | Số đo/raw thật | Không có mẫu thực tế mới | CHƯA KIỂM CHỨNG |
| ZIP/manifest | CRC, hash, khôngbin/obj/secrets/db | packaging.json/SOURCE_MANIFEST.sha256 | Xem kiểm đóng gói cuối |

Các lệnh / stdout / stderr / exit code thực tế ở Documentation/validation/ui-runtime-attempts.json. Trạng thái blocked không phải PASS build hoặc lỗi source đã được chứng minh. Báo cáo static không thay thế kiểm thử ứng dụng.

## Các ca hồi quy phải chạy trên máy .NET9 + SQL

| Nhóm | Case / expected | Test đã có / cách kiểm | Hiện tại |
|---|---|---|---|
| Admin login | Rỗng,/ hoặcHome→/Admin; validAdminGET phục hồi | RedirectTests + login-redirect-browser | Chưa |
| Staff/Driver/Customer login | /Staff,/Driver;Customer quay lạiTrip | Cùng tests, có CSRF/cookie độc lập | Chưa |
| Nhiều role | Admin→Staff→Driver→Customer | RedirectTests source link tới helper | Chưa |
| ReturnURL không an toàn | Ngoàiwebsite,//,encodedpath,dot,Login/Register/Logout/POST bị loại | Hai bộ test redirect | Chưa |
| Đã login mởLogin | Workspace đúng, khôngloop;Admin vẫn mởHome chủ động | login-redirect-browser | Chưa |
| Sai mật khẩu/IsActive/lockout | Thất bại đúng Identity, không chuyển workspace | Manual trên tài khoản test riêng | Chưa |
| Logout/quyền | POSTcsrf;workspace chặn saulogout;Customer/Staff/Driver khôngAdmin | login/browser scripts | Chưa |
| Khách | Home→search→trip→login→hold→confirm→vé | browser.mjs + manualHome/search | Chưa |
| Admin | Dashboardfilter,KPI/chart/empty;mọi list/formGET/POST/validation | ui-browser + manualCRUD trênDB test | Chưa |
| Staff | Tra cứu,bán hỗtrợ,linkCustomer,thu/hoàn,đổi/hủy,từchối,hànhkhách | SqlTests/browser + manual | Chưa |
| Driver | Chỉ chuyến giao,passengers,progress hợp lệ | SqlTests/browser | Chưa |
| Seat | Hai kháchtranh;refresh khônggia hạn,nhả,expiry,reconnect,token cũ | SqlTests/browser, contractsource giữ | Chưa |
| Đổi/hủy | Queryexchange không mất;rollback không mấtvé cũ;ledger không thayđổi | SqlTests + giữ queryLogin | Chưa |
| UX | 375/390/768/1024/1440;nooverflow;sidebar Escape/focus;ReduceMotion;200%zoom | ui-browser + manualzoom/keyboard | Chưa |
| HTTP | 400/401/403/404/409/503 đúng;không200giả | Browser/SQL tests + manualfailure | Chưa |

## Sửa lỗi tìm được trong vòng rà source

- Điều chỉnh browser helper cũ vốn chờ tất cả role vềHome; nay chờ đúngworkspace.
- Ngăn returnUrl trỏ actionPOST hoặc đường dẫnđãencode/dot; không chỉ checklocal.
- LinkLogin từTrip giữ cảquerystring để không mất tham số đổi vé.
- Confirm chuyển sangsubmit handler chung, không hỏi/xử lý hai lần; bỏ qua checkout-form do seats.js sởhữu.
- Biểu đồ không khởi tạo khi không cócanvas/no data;ReduceMotion đúng tùy chọn.
- Tăng tươngphản các nhãn tác nghiệp; có8 cặp đo tĩnh, chưa browser-audit.

Không kết luận đã hết500, đã đạtWCAG hoặc SQLdoublebooking0. Không sửa các báo cáo concurrency/performance baseline sangPASS. Screenshot thật sẽ được test script xuất khi chạy được trên SQL test; không có tệp ảnh giả trong ZIP.
