## Ảnh chụp local bổ sung — đối chiếu ngày 02/11/2026

Đã dùng công cụ web search được hỗ trợ và mở trang Commons của từng ảnh, đối chiếu giấy phép riêng; không khẳng định đã dùng Google Images trực tiếp. Tải file thực từ upload.wikimedia.org rồi tối ưu local. Không dùng thumbnail Google, ảnh FUTA, hotlink hay ảnh AI.

| File local thực | Nội dung | Tác giả và trang gốc | Giấy phép | Vị trí dùng |
|---|---|---|---|---|
| `wwwroot/images/buses/coach-hero.webp` | Xe khách đường dài màu trắng trên đường tại Espoo, Phần Lan — ảnh chụp | [VynedJ](https://commons.wikimedia.org/wiki/File:Mercedes-Benz_Tourismo_L%C3%A4nsiv%C3%A4yl%C3%A4_20260906.jpg) | [CC BY 4.0](https://creativecommons.org/licenses/by/4.0) | Hero trang chủ; Login/Register/Profile |
| `wwwroot/images/buses/coach-exterior.webp` | Xe khách đường dài Mercedes-Benz Tourismo tại Uetersen, Đức — ảnh chụp | [Frank Schwichtenberg](https://commons.wikimedia.org/wiki/File:Mercedes-Benz_O_350_Tourismo_01.jpg) | [CC BY 3.0](https://creativecommons.org/licenses/by/3.0) | Thumbnail tìm chuyến — ảnh minh họa chung |
| `wwwroot/images/destinations/da-lat.webp` | Hồ Xuân Hương, Đà Lạt — ảnh chụp | [Tu](https://commons.wikimedia.org/wiki/File:Xuan_Huong_Lake.jpg) | [CC BY 2.0](https://creativecommons.org/licenses/by/2.0) | Card điểm đến Đà Lạt |
| `wwwroot/images/destinations/nha-trang.webp` | Hòn Đỏ và chùa Từ Tôn, Nha Trang — ảnh chụp | [Dominic Nelson](https://commons.wikimedia.org/wiki/File:Nha_Trang_-_H%C3%B2n_%C4%90%E1%BB%8F_island_panoramic_Mar_2024.jpg) | [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0) | Card điểm đến Nha Trang |
| `wwwroot/images/destinations/ho-chi-minh-city.webp` | Đường chân trời TP. Hồ Chí Minh về đêm, ảnh năm 2011 — ảnh chụp | [Tri Nguyen](https://commons.wikimedia.org/wiki/File:Ho_Chi_Minh_City_skyline.jpg) | [CC BY 2.0](https://creativecommons.org/licenses/by/2.0) | Card điểm đến TP. Hồ Chí Minh |
| `wwwroot/images/banners/contact-journey.webp` | Đường đèo Hải Vân nhìn về phía bắc — ảnh chụp | [Vyacheslav Argenberg](https://commons.wikimedia.org/wiki/File:Hai_Van_Pass,_Vietnam,_View_northwards.jpg) | [CC BY 4.0](https://creativecommons.org/licenses/by/4.0) | Banner Liên hệ — minh họa hành trình, không gắn với tuyến trong dữ liệu |

Áp dụng cho từng file trong bảng: thu nhỏ, đổi WebP và bỏ EXIF; không sửa nội dung ảnh, không xóa logo/watermark. Không crop file gốc; khung card dùng object-fit theo vị trí. Chú thích tại ảnh có tác giả và link `/nguon-anh#...`; trang nguồn có link gốc, giấy phép và thông tin chỉnh sửa. Tác giả không bảo trợ/đại diện véxe. Xe chụp ở nước ngoài là minh họa chung, không được gán thành xe hay dịch vụ thật của chuyến. Không thêm tiện nghi/loại giường nằm dựa vào ảnh.

Riêng `nha-trang.webp` giữ CC BY-SA 4.0 cho bản WebP được phân phối. Các file còn lại giữ CC BY tương ứng. Việc tập hợp ảnh không tuyên bố thay giấy phép của toàn bộ source ứng dụng. File `Documentation/PHOTO_ASSETS.json` có URL tải, kích thước, dung lượng và SHA-256 từng ảnh.

Nguồn bị loại sau khi xem ảnh: `White bus in park.jpg` là xe nhỏ, không phù hợp xe khách đường dài; `Hai Van Pass.jpg` của Andre Hospers là ảnh cận thực vật, không phù hợp banner hành trình. Hai ảnh này không nằm trong ZIP bàn giao. Nguồn HCMC được chọn là ảnh năm 2011 (tên file skyline chữ thường), không dùng file Skyline chữ hoa năm 2024 có cảnh báo quyền về kiến trúc.

Hồ sơ tài nguyên cũ vẫn sử dụng được giữ ở bên dưới.

---

# Nguồn và quyền tài nguyên

| Tài nguyên | Phiên bản / nguồn | Quyền / cách dùng |
|---|---|---|
| bus-journey.svg, bus-small.svg, landscape.svg, favicon.svg | Tự vẽ bằng SVG cho project này | Minh họa gốc, cho phép dùng/sửa trong đồ án; không ảnh doanh nghiệp/sự kiện thật, không lấy logo/ảnh game |
| Bootstrap CSS / JS | 5.3.3, https://getbootstrap.com/ và npm bootstrap@5.3.3 | MIT; CSS/giấy phép chung tham khảo từ ZIP, JS upstream local; giữ LICENSE trong wwwroot/lib/bootstrap |
| SignalR JS | @microsoft/signalr 9.0.6, npm; https://github.com/dotnet/aspnetcore | MIT (.NET Foundation), minified browser bundle local; license upstream |
| Chart.js UMD | 4.4.9, https://www.chartjs.org/ và npm chart.js@4.4.9 | MIT, LICENSE local |
| CSS/JS ứng dụng | Viết mới cho BusBooking | Không lấy nghiệp vụ hoặc branding ZIP tham khảo |
| Typeface | system-ui/Segoe UI/Arial do hệ điều hành cung cấp | Không tải font ngoài |

Không hotlink hình. SVG ngang có width/height/viewBox, object-fit phù hợp. Hình chỉ minh họa, không mô tả nhà xe thật. Nếu ảnh lỗi, site.js thử fallback bus-small.svg một lần; alt vẫn có nếu fallback cũng thiếu. Không có ảnh người/khách thật.

Thư viện JS có thể kèm comment sourceMappingURL của upstream; website không cần sourcemap để chạy. Không có CDN bắt buộc ở runtime. NuGet vẫn cần mạng để restore lần đầu.


## Bản nâng UI01/11/2026

Thêm Views/Shared/_Icon.cshtml: SVG nét tự viết cho điều hướng/card, không tải/copy bộ icon Tabler/AdminKit. Giữ toàn bộ ảnh gốc local và giấy phép thư viện có sẵn; không thêm font/thư viện/template/ảnh ngoài. Không sử dụng ảnh/logo/mã nguồn FUTA/Vexere. wwwroot/css/ui.css,admin.css và js/admin.js là code gốc của lần sửa này.


## STOPS_SEATS

Các SVG path ghế/giường trong `wwwroot/js/seat-layout.js` và style booking.css do lần sửa này tự viết. Không tải/copy sprite, ảnh sơ đồ hoặc code của FUTA. Tất cả 10 file ảnh local đầu vào giữ byte nguyên vẹn; không thêm ảnh game, ảnh xe hoặc nguồn giấy phép mới. Xem validation/stops-review.json.


## BusGo — 03/11/2026

`wwwroot/images/busgo-mark.svg` và favicon là vector bus đơn giản tự tạo cho bản này, không sao chép logo nhà xe khác. Vector sở hữu bus-journey.svg chỉ đổi chữ hiển thị sang BusGo. Các ảnh WebP Commons, tác giả, URL nguồn và giấy phép được giữ nguyên; trang Nguồn ảnh và ghi công không bị xóa. Việc đổi nhãn không nhận ảnh coach minh họa là xe vận hành của BusGo.
