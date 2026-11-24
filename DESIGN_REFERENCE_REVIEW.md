# Khảo sát thiết kế — 03/11/2026

Nguồn đọc bằng công cụ web công khai, không đăng nhập/đặt vé/giữ ghế trên website tham khảo. Phiên này chỉ nhận prompt và ZIP mới; không giả vờ mở ảnh/Word nhận xét giảng viên từ phiên khác.

| Nguồn | Thực sự đọc được | Áp dụng riêng cho Bus Center |
|---|---|---|
| https://futabus.vn/ | HTML truy xuất chủ yếu có điều hướng; phần hero/app động không hiện đầy đủ trong bản trích xuất | Lấy ý tưởng ưu tiên tìm hành trình theo đề bài; không nhận đã khảo sát visual/booking live đầy đủ |
| https://futabus.vn/lien-he | Form công khai có họ tên/email/điện thoại/tiêu đề; thông tin FUTA tìm được thuộc FUTA | Hai cột thông tin và form, chỉ dùng thông tin cấu hình riêng |
| https://futabus.vn/huong-dan-dat-ve-tren-web | Hướng dẫn gồm tìm hành trình, chọn ghế/điểm/hành khách | Giữ trình tự thao tác hiện có; không chép thanh toán/khứ hồi/trung chuyển |
| https://vexere.com/ | Trang chủ timeout trong công cụ | Không tuyên bố đã xem phiên chọn chuyến live |
| https://vexere.com/vi-VN/huong-dan-thanh-toan-tren-website.html | Hướng dẫn website công khai, bước chọn ghế/điểm/hành khách | Chỉ tham khảo tổ chức thông tin; không thêm marketplace, voucher, bảo hiểm hoặc ví |

Không tải/clone logo/banner/HTML/CSS/hotline/dữ liệu doanh nghiệp FUTA/Vexere. Màu trắng–xanh và MVC/Bootstrap cũ được giữ. Bố cục mới tự viết, hero dùng ảnh local đã có quyền chứ không lấy banner đối thủ.

## Audit hero từ source đầu vào

Home đã có `hero-art` → `hero-photo` → `_Photo` và CoachHero; không phải thiếu markup ảnh. site/ui/photos-contact/busgo cùng tác động hero. Quy tắc cũ đặt tỷ lệ1600/1143/contain, max-height mobile235 rồi180!important, max-width riêng; card trừ38px để dành credit. Có nhiều quy tắc theo breakpoint, nhưng không có app đang chạy/ảnh mới để khẳng định nguyên nhân screenshot cụ thể hoặc việc tải ảnh thành công.

Bản sửa bỏ caption node và geometry dành credit; làm hero tự chứa hai cột, khung ảnh16:9, desktop minheight vừa phải, mobile xếp text–ảnh–form và ảnh display:block rõ. Eager/fetchpriority high chỉ ảnh hero; width/height/khung aspect-ratio dự phòng, ảnh dưới fold lazy, fallback local một lần. Không reveal che nội dung, không thư viện animation/carousel mới. Giữ nguyên GET Trips/Index origin/destination/date và dữ liệu tuyến.

Ảnh CoachHero đã được xem trực tiếp từ WebP local: xe trắng dài trên đường ở Espoo, Phần Lan. Khung object-position50%60% giữ phần xe chính, không xóa logo/watermark và không nói là đội xe Bus Center. CSS/DOM đã rà; việc ảnh/khung/overflow thực ở375/390/768/1024/1440 phải kiểm browser MVC sau build. Không có screenshot runtime trong ZIP này.
