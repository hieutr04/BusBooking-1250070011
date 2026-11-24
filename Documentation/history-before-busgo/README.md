## Cập nhật bản đang chạy — Ảnh & Liên hệ, 02/11/2026

## Bản STOPS_SEATS — 03/11/2026

Bản này sửa trực tiếp ZIP IMAGES_CONTACT_REVIEWED(9), giữ tên đề tài/chuyên ngành/.NET 9/SQL Server/Identity và ảnh/Liên hệ. Đọc **PICKUP_DROPOFF_GUIDE.md** trước khi mở bản mới với database cũ: cần migration tăng thêm `20261103000100_AddTripStops`, không chỉ giải nén và F5. Không xóa database hoặc chạy lại Initial/baseline.

Đã thêm điểm đón/trả theo chuyến + snapshot trên vé; nút **Đặt vé** vẫn Acquire 300 giây và confirm riêng; sửa total/checkout sau giữ chỗ; renderer 32 ghế một tầng hoặc 24 giường hai tầng dùng đúng TripSeatId, không thay capacity/hình học dữ liệu cũ. Giờ UTC+7, endpoint bám lịch gốc, giá toàn chuyến. Lịch trung gian mẫu là mô phỏng học tập, phải chạy command rõ ràng, không tự ghi đè.

**Trạng thái bàn giao: REVIEWED.** DOM test JavaScript có 10 tình huống PASS; kiểm tra cú pháp/asset đạt. SDK 9.0.305 đã tải nhưng CoreCLR không khởi tạo (0x8007000E, exit 137): restore/build/run, Razor, EF model diff/migration, SQL Server, HTTP/browser và responsive thực **CHƯA KIỂM CHỨNG**. Không có screenshot website MVC chạy thật; không chứng nhận hết lỗi 500. Log và bảng hiện tại ở TEST_RESULTS.md. Các kết quả cũ phía dưới là lịch sử, không thay thế báo cáo bản này.


Giữ nguyên .NET 9, solution BusBooking, thương hiệu véxe., chuyên ngành Thương mại điện tử và tên đề tài. Đây là source sửa trực tiếp từ ZIP UI_UPGRADED_REVIEWED(2), không phải website dựng lại.

### Với máy đã kết nối SQL và có tài khoản đang dùng

1. Dừng web bằng Shift + F5. Sao lưu source đang chạy và database: SSMS → database ứng dụng hiện tại → Tasks → Back Up → Full → OK, chờ báo thành công.
2. Giải nén ZIP mới. Mở `BusBooking/BusBooking.sln`. Giữ User Secrets và các giá trị môi trường hiện tại. `UserSecretsId` vẫn là `BusBooking-Development-2026`; các file appsettings, launchSettings, csproj và global.json trong ZIP được giữ nguyên byte từ ZIP đầu vào. Không đưa mật khẩu SQL vào file ZIP hay gửi ảnh có mật khẩu.
3. Có một migration tăng thêm `20261102000100_AddContactMessages`. Nó chỉ thêm bảng `ContactMessages` và hai chỉ mục; giữ nguyên Initial `20261029000100_InitialSqlServer`, snapshot V1 và lịch sử/baseline hiện có.
4. Nếu SQL login chạy web chỉ có quyền đọc/ghi (như tài khoản busbooking_app đã thiết lập), trong SSMS dùng tài khoản quản trị, **chọn đúng database ứng dụng hiện tại trên thanh công cụ**, mở `Database/20261102000100_AddContactMessages.sql`, chạy toàn bộ bằng F5. Kết quả cần là `CONTACT_MIGRATION_APPLIED` hoặc `ALREADY_APPLIED`. Script này là bản triển khai tăng thêm đã đối chiếu với C# migration; không phải SQL được sinh/chạy bởi dotnet ef trong môi trường bàn giao.
5. Tại thư mục có `.sln`, chạy kiểm tra:

```powershell
dotnet run --project BusBooking --launch-profile Https -- --check-db
```

Kết quả cần `SQL Server connected. Pending migrations: 0`. Không seed lại, không chạy lại Initial, không chạy `SCHEMA_REVIEW_ONLY.sql`, không sửa history/baseline cũ.
6. Chọn Https trong Visual Studio, nhấn F5. Thử `/lien-he`, gửi một câu hỏi thử; đăng nhập Admin → Liên hệ → xem đúng bản ghi → đánh dấu đã đọc.

Nếu dùng tài khoản triển khai đã có quyền DDL, có thể áp dụng bằng `dotnet ef database update --project BusBooking` thay cho script SSMS, sử dụng cấu hình hiện tại và đúng database. Không cấp thêm quyền DDL cho tài khoản chạy web chỉ để cập nhật. Không chạy cả hai như hai quy trình schema riêng; script có kiểm tra migration đã được áp dụng để chặn tạo trùng.

Nếu mở solution khi Windows chặn file tải về, Unblock chính ZIP đáng tin cậy trong Properties trước khi giải nén. Bộ công cụ EF vẫn dùng manifest `.config/dotnet-tools.json` hiện có.

### Các trang mới

- GET/POST `/lien-he`: công khai, lưu SQL, anti-forgery, xác thực dữ liệu và PRG; chưa tích hợp email/SMS.
- GET `/nguon-anh`: nguồn, tác giả, giấy phép và ghi nhận chỉnh sửa của 6 ảnh local.
- GET `/Admin/Contacts`, GET `/Admin/Contacts/{id}`, POST `/Admin/Contacts/{id}/read`: chỉ Admin.
- Thông tin hỗ trợ mặc định “Chưa cấu hình”. Tham khảo `CONTACT_PAGE_GUIDE.md` để điền giá trị thực qua mục Contact; không thay mục ConnectionStrings.

### Tình trạng kiểm chứng bản này

Đã thực hiện kiểm tra tĩnh/source/ảnh; **chưa kiểm chứng restore/build/Razor/DI, model diff trên EF runtime, SQL Server, HTTP và browser** trong môi trường bàn giao. SDK 9.0.305 có mặt nhưng CoreCLR không khởi tạo được (`0x8007000E`, exit 137). Không có SQL Server/Docker/sqlcmd để chạy database test. Chromium không có sẵn; lần tải browser trả ZIP hỏng. Không có screenshot runtime mới; không dùng mockup thay screenshot. Xem `TEST_RESULTS.md` và hướng dẫn test mới.

Các hướng dẫn cài mới và tài liệu lần bàn giao cũ phía dưới là lịch sử; với database đã chạy, làm theo phần cập nhật này.

---

# Bản nâng cấp UI trắng / xanh — 01/11/2026

**BusBooking/véxe. — giữ nguyên tên đề tài, nghiệp vụ và SQL Server.**

Mở `BusBooking.sln` như project hiện có. Đặt web project BusBooking làm Startup, profile SqlServer. Hành vi mặc định: Admin→/Admin, Staff→/Staff, Driver→/Driver, Customer phục hồi GET hợp lệ hoặc Home. Xem LOGIN_REDIRECT_FIX.md.

## Dùng với database đang có

1. Dừng app cũ; giải nén bản mới vào thư mục riêng, không chép đè file cá nhân một cách mù quáng.
2. Restore/build .NET9. Connection string User Secrets giữ cùng UserSecretsId `BusBooking-Development-2026`, nên khi dùng cùng tài khoản Windows các secrets đã lưu vẫn dùng được. Biến môi trường/config riêng nếu có phải giữ đúng cấu hình máy bạn, ZIP không chứa chúng.
3. Không thay schema/migration/seed. Database đã sẵn sàng thì không chạy seed hoặc tạo lại DB chỉ vì nâng UI. Chạy `dotnet ef migrations has-pending-model-changes --project BusBooking` để kiểm tra; **không chạy migrations add** cho bản UI này.
4. `dotnet run --project BusBooking --launch-profile SqlServer -- --check-db`; nếu connected và pending0, `dotnet run --project BusBooking --launch-profile SqlServer`, mở http://localhost:5190. Hoặc chọn SqlServer rồi F5, không chạy hai app cùng cổng.
5. Nếu Windows chặn manifest tải từ Internet: tại thư mục solution, `Unblock-File -LiteralPath ".\.config\dotnet-tools.json"` rồi `dotnet tool restore`. Không thay chính sách bảo mật toàn máy.

Tài khoản/mật khẩu hiện có không bị thay. Bảng tài khoản seed bên dưới chỉ áp dụng nếu trước đó đã seed Development; không phải yêu cầu seed lại. Nếu gặp SSPI, xử lý kết nối Windows/LocalDB, không drop/reset DB hoặc đổi backend.

## Trạng thái nghiệm thu lần nâng cấp

Có source UI34 Razor, layout tác nghiệp, redirect policy và bộ test. Môi trường lần này không khởi tạo được CoreCLR (exit137/HRESULT0x8007000E), không SQL Server/browser app, nên **restore/build/run/SQL/HTTP/browser/redirect runtime CHƯA KIỂM CHỨNG**. Không có screenshot thật. Kiểm syntax/static không thay thế runtime. TEST_RESULTS.md là báo cáo hiện hành; tài liệu baseline được giữ riêng.

- UI_PAGE_AUDIT.md:31 view baseline→34 view sau sửa, action/role/test.
- UI_CHANGELOG.md, DESIGN_REFERENCE_REVIEW.md, FILE_CHANGES.md: thiết kế và giới hạn.
- Tests/UI_TEST_GUIDE.md: policy test, login HTTP, browser và DB test riêng.

---

## Hướng dẫn cấu hình ban đầu từ baseline (chỉ dùng khi máy/DB chưa thiết lập)

# XÂY DỰNG HỆ THỐNG QUẢN LÝ XE KHÁCH VÀ ĐẶT VÉ TRỰC TUYẾN TÍCH HỢP CƠ CHẾ GIỮ CHỖ THEO THỜI GIAN THỰC

**Source ASP.NET Core MVC .NET 9 / SQL Server — bản REVIEWED, chưa VERIFIED.**

Tên kỹ thuật: BusBooking. Tên giao diện: véxe. Đây là ứng dụng đặt vé/điều hành xe khách mới, không chuyển đổi model hoặc migration từ ZIP tham khảo.

## Đọc trước khi chạy

- Đã đọc đầy đủ prompt Markdown đính kèm và rà cấu trúc ZIP tham khảo (4). Không nhận được Word đề cương và ảnh nhận xét ở phiên này, nên **chưa đối chiếu được hai căn cứ đó**. Không sửa Word, không bịa mục nhận xét bị cắt. Prompt gốc được giữ nguyên tại `Documentation/INPUT_REQUIREMENTS_ORIGINAL.md`.
- Đã có source MVC/Razor, EF SQL Server/Identity, migration và snapshot, service giao dịch, SignalR, worker, giao diện bốn vai trò, dữ liệu seed có kiểm soát, test và tài liệu.
- Môi trường tạo source không có SQL Server/Visual Studio. Đã tải SDK 9.0.305 nhưng host không khởi tạo được CoreCLR (HRESULT 0x8007000E, exit 137). **Chưa restore/build/run thành công, chưa kiểm HTTP/Razor/browser hay đo tranh chấp SQL Server.** Không xem kiểm tra cú pháp là bằng chứng ứng dụng chạy được. Xem `TEST_RESULTS.md`.
- Không có SQLite, EF InMemory, JSON backend, Preview, auto fallback, hosting, tunnel hoặc thanh toán ngân hàng.

## 1. Chuẩn bị máy Windows

Visual Studio 2022 **17.14**, workload **ASP.NET and web development**; .NET SDK **9.0.305** (hoặc feature band tương thích của .NET 9); SQL Server instance và SSMS. `global.json`: version 9.0.305, `rollForward=latestFeature`, không chọn .NET 10. Runtime/EF packages 9.0.9; SignalR JS 9.0.6; Bootstrap 5.3.3; Chart.js 4.4.9 đều local. Node/npm chỉ cần khi chạy browser test, không cần để chạy website.

Mở Terminal PowerShell tại thư mục chứa `BusBooking.sln`:

```powershell
dotnet --list-sdks
dotnet --info
dotnet tool restore
dotnet restore BusBooking.sln
dotnet build BusBooking.sln
```

Nếu máy không có SDK .NET 9: cài SDK 9 trước; không sửa `TargetFramework`, thư mục SDK hay `Microsoft.NET.Sdk.DefaultItems.targets`. Mở `BusBooking.sln`; chọn **BusBooking** (web) làm Startup Project, không chọn SqlTests.

## 2. Cấu hình đúng SQL Server

Mở SSMS → kết nối bằng instance đang dùng → ghi đúng **Server name**. Không suy đoán tên máy. Ví dụ bên dưới dùng LocalDB nếu bạn thực sự đã cài LocalDB. Thay bằng instance SSMS của bạn khi cần:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=BusBookingStudy;Trusted_Connection=True;TrustServerCertificate=True" --project BusBooking
dotnet ef migrations has-pending-model-changes --project BusBooking
dotnet ef database update --project BusBooking
dotnet run --project BusBooking --launch-profile SqlServer -- --seed
dotnet run --project BusBooking --launch-profile SqlServer
```

URL profile SqlServer: **http://localhost:5190**. Profile Https dùng https://localhost:7190 và http://localhost:5190 (`dotnet dev-certs https --trust` nếu cần). F5/Start trong Visual Studio với profile SqlServer sau migration/seed.

`has-pending-model-changes` là kiểm tra bắt buộc vì migration/snapshot khởi tạo được soạn trong môi trường không chạy được EF. Không bỏ qua cảnh báo hoặc xóa migration history để chạy tiếp. Nếu không khớp model, giữ DB hiện tại, xem log và sửa migration trên DB test trước.

Schema chỉ dùng EF migrations. Startup không tự migrate/seed/xóa DB. Không chạy `SCHEMA_REVIEW_ONLY.sql` chồng lên EF; file đó chỉ để đọc DDL. Nếu tài khoản thiếu quyền tạo DB, tạo database rỗng bằng SSMS và cấp quyền theo `SQL_SERVER_SETUP.md`. `dotnet run --project BusBooking --launch-profile SqlServer -- --check-db` kiểm kết nối và migration còn thiếu.

## 3. Tài khoản Development

Được tạo bằng UserManager/RoleManager khi chạy `--seed` ở Development. Mật khẩu chung **`BusStudy!2026`**, chỉ dành cho dữ liệu học tập:

| Vai trò | Email |
|---|---|
| Admin | admin@busbooking.test |
| Nhân viên | staff@busbooking.test |
| Tài xế | driver@busbooking.test |
| Khách hàng | customer@busbooking.test |

Seed không reset mật khẩu/dữ liệu đã có; mỗi lần bổ sung lịch thử trong 14 ngày tới theo ngày Việt Nam, tránh trùng khóa. Seed có 3 tuyến, 3 xe biển `TEST-*`, 2 sơ đồ, 42 chuyến trong lần tạo mới, phân công và một vé/thu tiền mô phỏng tạo qua service thật. Không phải lịch/giá/doanh nghiệp vận tải thật.

## 4. Thử nghiệp vụ

1. Customer tìm chuyến ngày mai, mở sơ đồ, chọn 1–6 ghế. Chọn trên UI chưa có quyền giữ.
2. Bấm **Giữ ghế**; server cấp hold 300 giây. Refresh không gia hạn; tab khác của cùng tài khoản phục hồi hold qua snapshot riêng tư.
3. Điền mỗi ghế một hành khách, xác nhận, xem mã vé và Vé của tôi. Vé chưa thu tiền vẫn Booked, không hết hạn theo hold cũ.
4. Staff mở Phòng vé → tra mã → xác nhận đã thu trực tiếp. Bán hỗ trợ dùng cùng màn hình/service giữ ghế, không có lối tắt cập nhật trạng thái. Nhân viên có thể nhập email Customer đang hoạt động để liên kết vé vào lịch sử khách; bỏ trống thì tài khoản phòng vé đứng tên, thông tin hành khách vẫn được lưu.
5. Customer gửi yêu cầu hủy/đổi trước khởi hành. Staff có thể từ chối có lý do; hoặc hủy/đổi nguyên tử ở Chi tiết vé. Đổi: chọn chuyến/ghế mới từ nút trong vé cũ, giữ rồi xác nhận. Vé cũ không mất nếu đổi thất bại.
6. Khoản đã thu của vé hủy/đổi ở trạng thái chờ hoàn; Staff ghi nhận hoàn sau khi đã xử lý trực tiếp. Vé mới có khoản thu riêng. Không tự gán mức phạt/phí chưa có căn cứ.
7. Admin quản lý danh mục/phân công, mở/đóng bán, hủy chuyến, tài khoản, nhật ký và báo cáo. Driver chỉ thấy chuyến được giao và chuyển chuẩn bị chạy → đang chạy → hoàn thành.

## 5. Kiểm thử SQL Server thật

Dùng **database riêng** có tên kết thúc `_Tests`, không dùng database học tập/người dùng. Harness không drop/reset DB; tạo fixture có mã riêng mỗi lần. Hãy dừng app học tập trước khi chuyển connection string.

```powershell
$env:SQL_TEST_CONNECTION="Server=(localdb)\MSSQLLocalDB;Database=BusBooking_Tests;Trusted_Connection=True;TrustServerCertificate=True"
dotnet run --project Tests/BusBooking.SqlTests -- --ack-test-db
```

Để test HTTP đồng thời, chạy app trên cùng DB `_Tests` ở Terminal A:

```powershell
$env:ConnectionStrings__DefaultConnection=$env:SQL_TEST_CONNECTION
dotnet ef database update --project BusBooking
dotnet run --project BusBooking --launch-profile SqlServer
```

Terminal B phải đặt cùng `SQL_TEST_CONNECTION` (biến môi trường theo từng terminal), rồi:

```powershell
dotnet run --project Tests/BusBooking.SqlTests -- --ack-test-db --http http://localhost:5190 --five-minutes
```

Đầu ra `TestResults/<run>/`: cases, fixture, raw JSON/CSV cho 20/50/100 request, thời gian trung bình/P95, kiểm DB, môi trường SQL/.NET thực. `--five-minutes` chờ 302 giây thật trong test, không giữ transaction/kết nối trong lúc chờ. Bỏ flag thì mục này ghi CHƯA KIỂM CHỨNG. Test fixture hết hạn được ghi rõ, không thay bằng hold vài giây.

Browser test: `Tests/README.md`. Kiểm restart process thực, quyền đầy đủ và các trường hợp fault-injection chưa được tự động hóa hết: xem ma trận test. Không lấy test tạo scope mới làm bằng chứng restart process.

## 6. Tài liệu & xử lý lỗi

- `SQL_SERVER_SETUP.md`: SSMS, quyền, backup, lỗi kết nối/migration.
- `HOLD_ALGORITHM.md`, `REALTIME_DESIGN.md`, `DATABASE_DESIGN.md`, `ERD.md`: thiết kế triển khai thực.
- `PERMISSION_MATRIX.md`, `REQUIREMENTS_TRACEABILITY.md`: quyền và đối chiếu yêu cầu.
- `TEST_RESULTS.md`, `CONCURRENCY_TEST_REPORT.md`, `PERFORMANCE_REPORT.md`: kết quả hiện có và phần còn chặn.
- `LIMITATIONS.md`, `DEFENSE_GUIDE.md`: giới hạn và giải thích khi bảo vệ.

Thiếu connection string: dừng startup với hướng dẫn; không chuyển DB giả. SQL Server không sẵn/schema thiếu: lỗi 503 có mã yêu cầu, log chi tiết phía server. Sai form 400, không có quyền 401/403, không có dữ liệu 404, tranh chấp 409. Mọi lỗi bất ngờ phải tra log, không khẳng định “hết lỗi 500” khi chưa chạy thực.
