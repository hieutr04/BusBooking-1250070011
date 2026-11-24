# Use case theo vai trò

| Actor | Use cases | Điều kiện / kết quả |
|---|---|---|
| Khách chưa đăng nhập | Tìm, xem chuyến/sơ đồ, đăng ký/đăng nhập | Không giữ ghế khi chưa đăng nhập |
| Customer | Hồ sơ, giữ/nhả/xác nhận, lịch sử/chi tiết, gửi hủy/đổi | Ownership ở service/controller; vé Booked không tự hết hạn |
| Staff | Tra cứu/bán hỗ trợ, thu/hoàn trực tiếp, xử lý đổi/hủy/từ chối, danh sách hành khách | Cùng giao dịch chống trùng như online; không sửa role/doanh thu tùy tiện |
| Driver | Lịch mình, danh sách hành khách chuyến giao, tiến trình | Check DriverId ở server; không nhìn hành khách mọi chuyến |
| Admin | User/role, tuyến/xe/layout/chuyến/giá/phân công, sales/cancel trip, dashboard/audit | Không xóa mất giao dịch; cấm lịch xe/tài xế chồng rõ ràng |

Luồng chuẩn: tìm chuyến → đăng nhập → chọn ghế → SQL giữ 300s → thông tin hành khách → SQL xác nhận trước hạn → mã vé → nhân viên thu trực tiếp. Chọn UI chưa phải hold, countdown chưa phải nguồn quyền. Các luồng thay thế được mô tả trong ACTIVITY_DIAGRAMS/SEQUENCE_DIAGRAMS và HOLD_ALGORITHM.
