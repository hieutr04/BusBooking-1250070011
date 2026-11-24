## 02/11/2026 — Ảnh thật và Liên hệ

- Home: ảnh coach local, card điểm đến theo tên/alias, ghi công, CTA Liên hệ; giữ nguyên GET tìm chuyến và dữ liệu tuyến.
- Trips/Index: thumbnail coach minh họa chung; giữ form, exchangeBookingId, giờ/giá/ghế và CTA cũ.
- Login/Register/Profile: ảnh mới trên desktop; giữ nguyên form/controller/returnUrl, ảnh ẩn trên mobile để ưu tiên thao tác.
- Header/footer/mobile: thêm Liên hệ; footer có Nguồn ảnh; active nav đúng /lien-he.
- Trang mới /lien-he hai cột/một cột, /nguon-anh, Admin tiếp nhận list/details/read; thêm icon mail nội bộ.
- Thêm photos-contact.css; site.js chỉ thêm active Liên hệ và fallback ảnh theo dữ liệu local, xử lý lỗi tải sớm và chặn loop. Không sửa CSS/JS cũ khác.
- Giữ tất cả dashboard Admin/Staff/Driver, vé, chính sách và sơ đồ ghế; không thêm ảnh quảng cáo vào dashboard. Chưa kiểm chứng browser/runtime; xem TEST_RESULTS.

---

# UI nâng cấp — trắng và xanh

## Khách hàng

Header/footer riêng cho trải nghiệm đặt vé, active navigation, menu mobile/Escape, account details bằng HTML native. Trang chủ đưa form tìm chuyến thành thao tác chính, hero gọn, tuyến theo model thật và hướng dẫn đặt vé. Không có hotline, rating hay giá gợi ý giả.

Kết quả chuyến có loại/xe, mốc thời gian, giá mỗi ghế và số khả dụng đúng dữ liệu. Chọn ghế có thanh bước, legend, trạng thái kết nối, khối tóm tắt và form phân đoạn. Mọi ID/data-attributes và seats.js giữ nguyên; login link giữ cả query để không mất exchangeBookingId.

Vé của tôi dùng thẻ vé; chi tiết bổ sung thu/hoàn/chờ hoàn từ ledger thật, giữ các điều kiện action cũ. Login/Register/Profile có autocomplete, label, validation và nút hiện/ẩn mật khẩu. Policy giữ nguyên nội dung chính sách, thêm intro/bố cục đọc. Problem dùng heading theo status, request ID và links GET thật.

## Điều hành

_AdminLayout riêng cho Admin/Staff/Driver và view dùng chung theo controller/role. Sidebar trái, header/breadcrumb/tài khoản/Xem website/POST logout, footer đề tài gọn. Ưu tiên menu Admin→Staff→Driver, không để Staff/Driver thấy menu Admin. Bootstrap offcanvas-lg cho mobile, Escape/backdrop/focus theo Bootstrap, không tự viết một modal chồng lên.

Admin dashboard: tiêu đề gọn, bộ lọc ngày, bốn KPI thật, biểu đồ thu/hoàn và đối soát, bảng doanh thu, giải thích ngày tính, lối tắt có backend. Khi không có ledger trong kỳ thì empty state; không tạo số mẫu. Không sửa ReportService hoặc công thức.

Toàn bộ trang danh mục, bảng/form, tài khoản, nhật ký có typography/spacing/toolbar/helper/empty state thống nhất. Form nhóm fieldset và savebar. LayoutEdit có preview từ giá trị đang nhập, ghi rõ chưa lưu; không đổi quy tắc lưu/server. Phòng vé có lối bán hỗ trợ và tra cứu thật, đổi/trả giữ workflow cũ. Driver có lịch/hành khách/tiến trình, không mở rộng dữ liệu ngoài quyền.

## Kỹ thuật trình bày

- Thêm ui.css cho tokens/public/shared và admin.css chỉ ở workspace. site.css baseline giữ làm nền tương thích các component hiện có, không nhập template ngoài.
- site.js: menu mobile/Escape, account menu, active link, password toggle, confirm trong submit và trạng thái đang gửi, phục hồi khi back/pageshow. Không đăng ký hai handler cho cùng thao tác.
- admin.js: đồng bộ aria-expanded offcanvas, preview layout chưa lưu.
- dashboard.js: giữ Chart.js và dữ liệu thật, palette xanh, tooltip VND, reduce motion, guard khi chart không có dữ liệu.
- Không sửa seats.js, API hay service transaction. Không có animation che nội dung; transition nhẹ ở color/background/transform/box-shadow, hỗ trợ reduced motion.
- CSS breakpoints cho375/390/768/1024/1440 và vùng cuộn bảng riêng. Đây là thiết kế trong source; chưa được browser QA ở các viewport này.
- Thêm partial icon SVG nét gốc, giữ ảnh local đã có; ảnh tuyến lazy-load. Không thêm CDN/font/icon library.

## Giới hạn

Chưa restore/build/run/browser được trong môi trường này; không có screenshot thật của bản nâng cấp. Không khẳng định đã đạt contrast/accessibility hoặc không còn500. Giữ giới hạn100/200 bản ghi baseline; chỉ ghi đúng số bản ghi đang hiển thị, không thêm ô lọc giả. Không có pagination mới hoặc nghiệp vụ mới.


## STOPS_SEATS — 03/11/2026

- Thêm booking.css, seat-layout.js, stop-editor.js, layout-previews.js; sửa seats.js, admin.js và thêm load asset trong hai layout. Không tải thư viện giao diện mới.
- Trips/Details: chọn điểm đón/trả có giờ VN, tóm tắt, lời nhắc đến sớm và cờ lịch học tập. CTA Đặt vé vẫn giữ 300 giây, nút confirm riêng; Tiếp tục đưa về form.
- Sửa summary lấy activeHold thay vì selected đã clear; cập nhật nhiều hold/draft, refresh và không reset expiry. Explicit [hidden] tránh phần tử ẩn bị rule display ghi đè.
- Renderer chung có icon tự vẽ, đầu xe/lối đi/tầng, không đổi DB geometry. Admin layout edit và preview đã lưu cùng renderer.
- Admin/Trips thêm link Điểm đón/trả, View TripStops có validation, revision và khóa lịch theo hold history. Booking Details/Index/Passengers hiển thị snapshot; không truy cập lịch sử điểm công khai.
- Breakpoint: 768–1100 một cột đặt vé; mobile hai tầng xếp dọc. Actual browser/responsive chưa kiểm chứng; DOM test không thay bằng chứng này.
