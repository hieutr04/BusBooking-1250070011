# Tích hợp ảnh thật — 02/11/2026

XÂY DỰNG HỆ THỐNG QUẢN LÝ XE KHÁCH VÀ ĐẶT VÉ TRỰC TUYẾN TÍCH HỢP CƠ CHẾ GIỮ CHỖ THEO THỜI GIAN THỰC

## Thay đổi trình bày

- Trước: hero/auth dùng bus-journey.svg, danh sách chuyến dùng bus-small.svg, card tuyến đều landscape.svg.
- Sau: hero/auth dùng ảnh coach thật có ghi công; thumbnail chuyến dùng ảnh coach thứ hai, được tối ưu 640px; card tuyến chọn ảnh đúng tên điểm đến; Liên hệ dùng ảnh đường đèo Hải Vân nhìn về phía bắc.
- Giữ logo, favicon, icon, sơ đồ ghế, SVG fallback và toàn bộ CSS/JS cũ. Thêm photos-contact.css; không thay seats.js hay hợp đồng API giữ ghế.

| File | Kích thước gốc | Kích thước local | Dung lượng | Vị trí |
|---|---|---|---|---|
| `coach-hero.webp` | 4043×2888 | 1600×1143 | 392,934 byte | Hero trang chủ; Login/Register/Profile |
| `coach-exterior.webp` | 6000×4000 | 640×427 | 31,856 byte | Thumbnail tìm chuyến — ảnh minh họa chung |
| `da-lat.webp` | 1280×756 | 1280×756 | 115,392 byte | Card điểm đến Đà Lạt |
| `nha-trang.webp` | 10528×3666 | 1600×557 | 144,940 byte | Card điểm đến Nha Trang |
| `ho-chi-minh-city.webp` | 1500×454 | 1500×454 | 53,582 byte | Card điểm đến TP. Hồ Chí Minh |
| `contact-journey.webp` | 3984×2656 | 1600×1067 | 62,598 byte | Banner Liên hệ — minh họa hành trình, không gắn với tuyến trong dữ liệu |

Tổng ảnh mới: 801,302 byte, 6 ảnh WebP. Không đưa các bản gốc nhiều MB vào ZIP.

## Mapping và fallback

`Services/PhotoCatalog.cs` chuẩn hóa dấu tiếng Việt, chữ hoa/thường, khoảng trắng/dấu chấm và các alias TP.HCM/Sài Gòn. Chỉ khớp chính xác ba địa danh đã xác minh theo dữ liệu tuyến hiện có. Không dùng ID seed. Tuyến mới có địa danh khác vẫn được tìm/đặt vé bình thường; card dùng SVG trung tính với alt nêu rõ chưa có ảnh chụp phù hợp, không gán nhầm địa điểm. Không đổi schema tuyến/xe/chuyến để lưu ảnh.

Ảnh có width/height, object-fit contain cho xe để giữ đầu/đuôi xe; card địa danh dùng cover với điểm neo riêng (TP.HCM 20% center để giữ tòa tháp bên trái, Đà Lạt 50% 60%, Nha Trang giữa ảnh). Hero eager/high priority; ảnh dưới màn hình đầu lazy. Auth ẩn ảnh ở màn hình nhỏ để ưu tiên form. Dùng local landscape.svg khi lỗi; site.js chặn vòng lặp fallback và xử lý cả ảnh lỗi trước khi handler được đăng ký. Tất cả src của ảnh là local; nguồn/giấy phép chỉ là liên kết tham khảo, không được tải để hiển thị ảnh.

## Kiểm chứng và giới hạn

Đã mở contact sheet từ ảnh tải thật để kiểm tra chủ thể; loại hai nguồn không đúng chủ đề rồi tải thay thế. Đã giải mã ảnh bằng Pillow, kiểm kích thước, hash và dung lượng. Đây là kiểm tra file ảnh, không phải screenshot ứng dụng.

Chưa có .NET runtime/SQL/browser hoạt động để kiểm crop và overflow trên trang Razor thực ở 375/390/768/1024/1440px; các mục này CHƯA KIỂM CHỨNG. Có test tự động mới để chạy trên máy có SQL test. Không bàn giao screenshot mockup.
