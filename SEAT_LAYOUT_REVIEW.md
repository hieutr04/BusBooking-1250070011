# Sơ đồ ghế/giường

Renderer chung `wwwroot/js/seat-layout.js`, CSS `booking.css` dùng trong trang chuyến, preview layout đã lưu và form cấu hình Admin. Icon SVG nét do lần sửa này tự vẽ bằng path; không dùng ảnh/sprite/HTML/CSS FUTA. Chỉ tham khảo nguyên tắc icon, hai tầng và lối đi.

| Dữ liệu hiện tại | Cách hiển thị | Bảo toàn |
|---|---|---|
| 32 chỗ, 8 × 4, một tầng | 2 ghế — lối đi — 2 ghế; đầu xe/tài xế minh họa | 32 TripSeatId/mã A01–A32 như dữ liệu |
| 24 chỗ, 4 × 3 × 2 tầng | 12 giường/tầng, ba dãy tách lối đi, Tầng dưới/Tầng trên | 24 ID/mã theo dữ liệu, không đổi thành 34 chỗ FUTA |

Renderer phân nhóm/sắp xếp bằng Floor/Row/Column của TripSeat snapshot. Track lối đi chỉ tồn tại trong CSS Grid; không thêm ghế vật lý/ghế chuyến. Chọn ID một-một với nút. Ghế đã đặt/giữ bởi người khác không chọn được, có label trạng thái, keyboard focus và aria-label. Một tầng dùng icon ghế, nhiều tầng dùng icon giường theo hai loại seed hiện có. Không khẳng định bố cục học tập là thông số một xe thương mại cụ thể. Nếu tương lai có xe ghế ngồi hai tầng hoặc layout đặc biệt, cần metadata kiểu xe/renderer tương ứng; phiên bản này không âm thầm suy đoán lại DB.

Hai tầng cạnh nhau trên desktop, xếp dọc trên mobile. Tại tablet 768–1100px khối đặt vé chuyển một cột để giữ kích thước nút. Không dùng tab nên chọn ghế khác tầng không bị ẩn mất. Không thay Code/PhysicalSeatId/Row/Column/Floor hoặc hình học bất biến của chuyến cũ.

JS DOM đã kiểm count/ID/code/tầng/lối đi, actual CSS visual responsive/browser **CHƯA KIỂM CHỨNG**. Không có screenshot Razor/SQL chạy thật trong bản bàn giao do lỗi runtime môi trường; không cung cấp mockup thay thế.
