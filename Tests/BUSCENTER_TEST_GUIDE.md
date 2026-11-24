# Chạy kiểm chứng Bus Center

1. Build và helper/policy/model-diff theo TEST_RESULTS.md. Nếu build/Razor/DI lỗi, ghi FAIL và lỗi thật; chưa được đổi tên ZIP thành VERIFIED.
2. Dùng SQL riêng `_Tests` theo Tests/README.md; giữ fixture/harness cũ20/50/100 và --five-minutes. Không xóa/reset DB thật của người dùng. Thử specific endpoint khi có active/expired/confirmedhold và customstop; compare ID/offset/order/name/address/flags/geometry/fare/assignments/holdexpiry/booking snapshot trước-sau; chạy lại/đồng thời/stale revision. SaiArea/Source/checkbox/cờ/TripId/ngược chiều bị chặn; không renumber/xóa oldrows.
3. Contact thực: validPOST có row trước PRG success, lặp cùngSubmissionKey không trùng, payloadkhác409; invalid400/rate429/SQLpermission503 thật; Admin đọc/đánh dấu, Customer/Staff/Driver không được đọc; XSS encode. Điền cấu hình test riêng (example.test/điện thoại giả) để thửlinkhợp lệ, không dùng dữ liệu FUTA hoặc cá nhân thật.
4. Tạo fixtureSQL bằng harness --fixtures-only hoặc chạy full suite để có fixtures.json. App localhost phải trỏ cùng `_Tests`, có tripfixture. Tại Tests:

```powershell
npm install
npx playwright install chromium
$env:BASE_URL="http://localhost:5190"
$env:FIXTURES="C:\duong-dan-thuc\TestResults\<run>\fixtures.json"
npm run buscenter-browser
```

Script chụp Home/Contact/Trips-Details thật ở375/390/768/1024/1440 và ghiresults.json. Không mởappđược thì FAIL/CHƯA, không chạy staticHTML thay. Các script login-redirect-browser/browser/ui-browser trước vẫn dùng cho4role/CSRF/holdconfirm/history/keyboard/workspace. Browsermới không tạo booking bên website tham khảo.
5. Kiểm thủ công Admin SpecificStopPOST khôngCSRF/rolekhác; stale revision; sourceDetail500unicode khôngtràncột; customsameName khácđịachỉ/offset chặn. SnapshotgiữriêngpointIDs từnghold; điểmoldchỉ dànhholderkhôngđượcofferedchoholdkhác; release/expire/removehold xóalựachọnoldkhỏiUI. Fare/expiryđúng, đổivéatomic/thuhoàngiữnguyên.
6. Home5viewport: ảnhnaturalWidth>0, khôngcắt mất xe, formGETgiữbinding, khôngcaption/bordertrống, khôngoverflow/CLS lớn, reducedmotion/menu/focus. Contact2cột/1cột, footerthôngtinđồngbộ, invalidtel/URLkhônghiện. TấtcảbrandBus Center, creditscôngkhaiđủnguồn. LưuHTTP/console/log/screenshotactual từdữliệugiả; khônglưuthôngtincánhân thật.

DOM fixture đang chạy được: npm run seat-ui; npm run stop-editor; npm run contact-dom. PASS DOM không thay SQL/browser.
