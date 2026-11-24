# Liên hệ: biểu mẫu SQL và hộp tiếp nhận Admin

## Route và quyền

| Route | Method | Quyền | Tác dụng |
|---|---|---|---|
| /lien-he | GET | Guest và mọi vai trò | Hiện form, tạo token gửi có hạn 2 giờ; không ghi SQL |
| /lien-he | POST | Guest và mọi vai trò | Kiểm dữ liệu/anti-forgery/rate limit, lưu SQL và redirect |
| /Admin/Contacts | GET | Admin | 20 liên hệ/trang, bộ lọc chưa đọc |
| /Admin/Contacts/{id} | GET | Admin | Xem nội dung đã encode; GET không đánh dấu đã đọc |
| /Admin/Contacts/{id}/read | POST | Admin | Đánh dấu đã đọc, có anti-forgery, không gửi email |
| /nguon-anh | GET | Công khai | Hồ sơ ghi công ảnh, không có thông tin liên hệ của người gửi |

AdminContactsController có Authorize(Roles="Admin") ở cấp class. Các vai trò Staff/Driver/Customer và Guest không có quyền xem list/details/mark-read, dù biết ID. Sidebar chỉ hiển thị mục Liên hệ cho Admin. Không thêm vai trò, không thay logic đăng nhập/returnUrl cũ. Không có API công khai tra liên hệ.

## Field và phản hồi

- Họ tên: bắt buộc, 2–100 ký tự; email: bắt buộc, đúng định dạng, tối đa 254.
- Điện thoại: tùy chọn, tối đa 25 ký tự, 7–15 chữ số; chỉ cho số, + đầu, khoảng trắng, (), dấu - và .
- Tiêu đề: bắt buộc 3–160; nội dung: bắt buộc 10–4.000 ký tự. Bỏ khoảng trắng đầu/cuối trước lưu; kiểm tối thiểu sau trim.
- Token gửi: Data Protection, không cho giả SubmissionKey; hạn 2 giờ. Không dùng token này thay anti-forgery.
- Có label, lỗi từng field, summary, native HTML fallback khi JS tắt, kiểm tra client riêng, counter, loading và focus.
- Chỉ sau SaveChangesAsync thành công mới thông báo “Đã ghi nhận liên hệ trong hệ thống.” và redirect GET. Refresh GET không gửi lại form.
- Lỗi dữ liệu trả 400; replay có nội dung khác trả 409 và cấp token mới để người gửi chủ động gửi lại; lỗi SQL trả 503, giữ nội dung hợp lệ trong form. Không báo đã gửi email.
- Giới hạn 64 KiB/request, 12 form values, 12.000 ký tự/value; POST liên hệ có quota 8 request/IP trong 10 phút, không xếp hàng. HTTP 429 có hướng dẫn chờ/Quay lại và Retry-After nếu limiter cấp metadata.

## SQL, idempotency và riêng tư

Một bảng ContactMessages, không có FK đến Identity/booking. SubmissionKey có unique index. Gửi lại cùng token và nội dung trả cùng kết quả thành công nhưng không tạo dòng mới; race giữa hai request được unique index xử lý. Thay nội dung trên token đã dùng bị từ chối; không trả ID/nội dung bản ghi cũ ra công khai.

Lưu UTC, Admin hiển thị UTC+7 bằng BusinessTime. Nội dung hiển thị bằng Razor encode, không Html.Raw. Logger của ContactController chỉ ghi mã SQL và request ID khi lỗi, không ghi nội dung hay email/điện thoại. Không bật EF sensitive data logging. Danh sách Admin không lấy cột Body khi phân trang.

Quota nhẹ ở một tiến trình, theo RemoteIpAddress; không tin X-Forwarded-For. Đây không phải giải pháp chống spam phân tán/CRM. Không thêm SMTP, SMS, chat, workflow phân công hoặc phản hồi hàng loạt.

## Cấu hình thông tin hỗ trợ

Ba khóa riêng: Contact:SupportEmail, Contact:SupportPhone, Contact:SupportAddress. Ví dụ cấu trúc không có thông tin giả ở Documentation/CONTACT_CONFIG_EXAMPLE.json. Có thể thêm mục Contact vào appsettings.Development.json hoặc dùng Manage User Secrets, giữ nguyên mục ConnectionStrings đang dùng. Chỉ điền thông tin có thật do bạn sở hữu/có quyền công bố.

Mặc định ba giá trị rỗng: UI hiện “Chưa cấu hình”, không tạo mailto/tel giả. Email và điện thoại sai định dạng cũng không tạo link. Thương hiệu hiển thị “véxe. — hệ thống đồ án”, không giả công ty vận tải đăng ký.

## Triển khai và thử

Đọc DATABASE_CHANGELOG.md, backup và áp dụng migration mới trên database hiện tại. Không seed lại. Nếu thiếu bảng, form không báo thành công giả; thông báo 503 hướng dẫn kiểm tra migration.

Tests/contact-browser.mjs chạy trên SQL TEST riêng với cờ BB_ALLOW_TEST_WRITES=YES; nó tạo một bản ghi tổng hợp và cần 4 tài khoản test. Tests/CONTACT_TEST_GUIDE.md nêu cách chạy, kiểm lỗi DB, payload, quyền và ảnh. Không có kết quả runtime PASS sẵn trong bản này.
