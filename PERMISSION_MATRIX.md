## Bổ sung Liên hệ — 02/11/2026

| Chức năng | Guest | Customer | Staff | Driver | Admin |
|---|---|---|---|---|---|
| Mở/gửi /lien-he | Có | Có | Có | Có | Có |
| /nguon-anh | Có | Có | Có | Có | Có |
| Xem danh sách/chi tiết liên hệ | Không | Không | Không | Không | Có |
| POST đánh dấu đã đọc | Không | Không | Không | Không | Có + anti-forgery |

Không thêm vai trò. Các quyền cũ giữ nguyên. Bảo vệ controller đã rà tĩnh; HTTP RBAC chưa được chạy ở môi trường bàn giao.

---

# Ma trận quyền ở server

Guest là khách chưa đăng nhập, không phải role thứ năm. Mỗi user một trong Admin/Staff/Driver/Customer. Tài khoản mới chỉ được Customer. Cookie Identity HttpOnly, SameSite Lax; production HTTPS, security stamp kiểm mỗi request. Khóa tài khoản/đổi role làm principal cũ mất hiệu lực.

| Tác vụ | Guest | Customer | Staff | Driver | Admin | Bằng chứng |
|---|---|---|---|---|---|---|
| Tìm chuyến/snapshot công khai | ✓ | ✓ | ✓ | ✓ | ✓ | Trips/SeatsApi GET, không PII |
| Hồ sơ cá nhân | — | Của mình | Của mình | Của mình | Của mình | Account.Profile dùng UserManager.GetUserAsync |
| Giữ/nhả/xác nhận | — | Hold của mình | Hold của mình/phòng vé | — | Hold của mình | SeatsApi roles; service kiểm owner/hạn |
| Chi tiết vé | — | Vé mình | Mọi vé để tác nghiệp | — | Mọi vé | Bookings.Details kiểm owner hoặc Staff/Admin |
| Yêu cầu hủy/đổi | — | Vé mình | Vé đứng tên mình qua service | — | Vé đứng tên mình qua service | RequestChange kiểm OwnerId |
| Thực hiện hủy/đổi, thu/hoàn | — | — | ✓ | — | ✓ | StaffController; Confirm exchange cần privileged=true từ role |
| Hành khách theo chuyến | — | — | ✓ | Chuyến được giao | ✓ | Driver.Details kiểm DriverId trước query PII |
| Tiến trình chuyến | — | — | — | Chuyến được giao | Không dùng route Driver | Driver.Progress + service DriverId |
| Mở/đóng bán/hủy chuyến | — | — | — | — | ✓ | Admin POST + service gate |
| Danh mục/phân công/role/nhật ký/report | — | — | — | — | ✓ | AdminController Authorize |
| Hub Join/ACK | Read only | Read only | Read only | Read only | Read only | Trip tồn tại / đúng group, không bán vé |

Mọi POST MVC/API có AutoValidateAntiforgeryToken; fetch dùng X-CSRF-TOKEN từ `/api/seats/csrf`. DTO không có giá/role có thể tự cấp qua register. Dữ liệu SQL tham số hóa bằng EF/FromSqlInterpolated/ExecuteSqlInterpolated. Razor encode tên/lý do/người dùng; JS tạo DOM bằng textContent, không đưa PII vào innerHTML.

API anonymous/sai role trả 401/403 thay redirect. Trang web chuyển Login hoặc trang 403. IDOR check ở server, không chỉ ẩn menu. API snapshot no-store tránh cache MyHolds; chi tiết/tác nghiệp authenticated no-store. Seed test không có người dùng/PII thật.

Giới hạn: chưa có kiểm thử runtime đầy đủ, chưa có password reset/email xác thực/chống spam nâng cao vì không nằm trong phạm vi đã đăng ký. Không tuyên bố pentest đạt.


Điều hướng sau login được giới hạn bởi LoginDestination (LOGIN_REDIRECT_FIX.md); không thay authorization/ownership của bảng quyền này. Layout tác nghiệp chọn menu Admin→Staff→Driver khi nhiều role, không thay dữ liệu role.


## Điểm đón/trả

- GET /api/seats/{tripId}: lịch điểm công khai của chuyến; MyHolds/điểm đã chọn chỉ của actor đăng nhập.
- POST /api/seats/stops: Customer/Staff/Admin + chủ hold, anti-forgery, hold active/còn hạn; Driver/Guest không được ghi.
- GET/POST /Admin/TripStops/{id}: Admin, POST anti-forgery + revision + không có hold history. Không thay quyền của Staff/Driver.
- Snapshot điểm trên vé tuân theo quyền/ownership Bookings và hành khách đã có; không public danh sách người chọn điểm.
