# Cấu hình Liên hệ Bus Center

`BusBooking/appsettings.json` chứa địa chỉ, email, hotline và điện thoại do người dùng cung cấp. Trang Liên hệ và các footer đọc chung cấu hình này.

Các khóa bind từ `Contact` (không có schema mới):

| Khóa | Nội dung cần chủ website cung cấp |
|---|---|
| LegalName | Tên pháp nhân nếu có; bỏ trống nếu chưa có |
| SupportAddress | Địa chỉ hỗ trợ thực tế đã được phép công bố |
| SupportWebsite | URL http/https thuộc website đang vận hành; không bịa buscenter.vn |
| SupportPhone | Điện thoại thực,7–15 chữ số, có thể gồm + và ký tự định dạng |
| SupportHotline | Hotline, lưu riêng với điện thoại |
| FacebookUrl | Liên kết HTTPS đến trang Facebook của Bus Center |
| ZaloUrl | Liên kết HTTPS đến kênh trên zalo.me của Bus Center |
| SupportEmail | Email nhận phản hồi thực, không sao chép email của FUTA |
| SupportHours | Giờ hỗ trợ thực; không mặc định24/7 |

Trong Visual Studio: chuột phải **project BusBooking → Manage User Secrets**. Giữ toàn bộ ConnectionStrings và các khóa đang dùng; chỉ hợp nhất section Contact theo `Documentation/CONTACT_CONFIG_EXAMPLE.json`, rồi điền các giá trị thật. Không thay cả file secrets bằng ví dụ, không gửi ảnh có mật khẩu và không đóng gói secrets. Cũng có thể dùng cấu hình môi trường `Contact__SupportEmail`... hoặc appsettings riêng khi triển khai; source appsettings gốc được giữ nguyên byte.

Trang Liên hệ và footer dùng cùng `_SupportChannels`/ContactOptions. Email/điện thoại/URL không hợp lệ không tạo mailto/tel/link; URL javascript/data hoặc có user-info bị loại. Website mở tab mới có noopener/noreferrer. Facebook và Zalo hiển thị biểu tượng. Điền URL chính thức để bật liên kết.

Form giữ Họ tên/Email/Điện thoại/Tiêu đề/Nội dung, kiểm tiếng Việt, focus/loading; chỉ báo đã ghi nhận sau Record/SaveChangesAsync hoặc retry cùng submission có dữ liệu khớp. Không có SMTP nên không nói đã gửi email. ContactService/Controller/Tokens/schema/giới hạn gửi/quyền Admin giữ nguyên byte. Chưa kiểm chứng SQL thực của bản mới; cần thử ghi/đọc Admin/duplicate/CSRF/permission/503 trên DB test theo TEST_RESULTS.
