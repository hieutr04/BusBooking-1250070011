# Kiểm bản UI nâng cấp trên máy có .NET 9 và SQL Server

Không có kết quả runtime của lần sửa này. Chỉ chạy ghi dữ liệu trên DB riêng kết thúc `_Tests`. Không thay connection/user-secrets của DB hiện dùng chỉ để chạy test. Cách tạo fixtureSQL thật ở Tests/README.md; một terminal app dùngConnectionStrings__DefaultConnection trỏ đúng DB test, một terminal harnessSQL_TEST_CONNECTION trỏ cùngDB.

## 1. Build/model và helper redirect không SQL

```powershell
dotnet --info
dotnet tool restore
dotnet restore BusBooking.sln
dotnet build BusBooking.sln
dotnet ef migrations has-pending-model-changes --project BusBooking
dotnet run --project Tests/BusBooking.RedirectTests
```

RedirectTests link trực tiếp LoginDestination.cs, không có bộ policy môphỏng sao lại. Output JSON expected/actual từng case, exit1 nếu lỗi. Bản UI không thêm migration.

## 2. Fixture và bộ chức năng cũ

Theo Tests/README.md để tạoSQL DB `_Tests`; dùngSDK/SQL thực, không SQLite/RAM. Tạo fixturemới:

```powershell
dotnet run --project Tests/BusBooking.SqlTests -- --ack-test-db --fixtures-only
```

Sau đó ởthưmụcTests:

```powershell
npm install
npx playwright install chromium
$env:FIXTURES="C:\duong-dan-thuc\TestResults\<run>\fixtures.json"
$env:BASE_URL="http://localhost:5190"
node login-redirect-browser.mjs
node browser.mjs
node ui-browser.mjs
```

- login-redirect-browser.mjs: HTTP cookie/CSRF,4 roles,returnUrl/default/authenticated Login,AdminHome,POST logout. Kếtquảlogin-redirect-results.json.
- browser.mjs: giữ hold/confirm/reconnect/IDOR/CSRF gốc,thêm390px vàworkspace login đúng. Tạo mộtvétest choCustomer fixture.
- ui-browser.mjs: GETcácnhómPublic/Admin/Staff/Driver ở5 viewport,screenshot thật,sidebar Escape/focus,passwordtoggle,previewlayout chưa lưu,ReduceMotion;xuấtui-results/results.json vàPNG. NếuCustomerchưa có vé thìscreenshot chi tiết vé được ghiCHƯA,khôngPASSgiả.

Tests chỉ nhận fixturetên DB `_Tests`; dùng thông tin hành khách/tài khoản giả. Không chạy và chụp dữ liệu cá nhân thật. Không tự upload ảnh/log ra ngoài. Fixtuređược giữ làm lịch sửtest;khôngreset DB người dùng.

## 3. Bộghi SQL/concurrency/hạn300s

Giữharnessgốc, chạynhưTests/README.md:

```powershell
dotnet run --project Tests/BusBooking.SqlTests -- --ack-test-db --http http://localhost:5190 --five-minutes
```

Raw20/50/100JSON/CSV,worker/ownership/đổi/hủy/thu hoàn,5phút thật. Không lấyread-load/kịch bản tĩnh làm bằng chứngwriteconcurrency. Hiện các báo cáo sốđo vẫn CHƯA KIỂM CHỨNG.

## 4. Thủ công

Kiểm tất cả AdminCRUDGET/POST,role/IsActive trênfixture,Dashboarddatekhônghợp lệ/zero,Staffworkflow,Driverprogress,loginwrongpassword/lockout/inactive,multiple-roleuser trongDB test,HTTPstatus,zoom200%,keyboard/disabled/loading/failure,ảnhfallback. Khôngđổi dữ liệu role sản phẩm để làm test redirect. ChỉghiPASS khi cóactual/log/ngày/máy/phiênbản.
