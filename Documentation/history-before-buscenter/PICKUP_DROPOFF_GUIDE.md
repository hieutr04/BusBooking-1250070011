# BusGo — điểm đón/trả và cập nhật dữ liệu hiện có

## Nguyên nhân kiểm tra được từ source

`TripStopMaintenance.StudyTemplate` đầu vào đã có chiều Đà Lạt → TP. Hồ Chí Minh 420 phút: Đức Trọng +90, Bảo Lộc +180. Nhánh cũ yêu cầu đúng tên/mô tả học tập/biển TEST/lịch chỉ có endpoint và không có **bất kỳ** SeatHold nào. Một hold đã hết hạn vẫn khóa nhánh đó. Đây là nguyên nhân có thể gây thiếu điểm từ source; chưa truy cập database máy người dùng nên không khẳng định TripId cụ thể đã gặp điều kiện nào.

Service mới nhận alias tên tuyến nhưng không bỏ guard sửa/xóa lịch đã có hold. Chỉ nhánh **thêm điểm mới** được phép trên chuyến Scheduled, chưa đến giờ đi, có lịch nhất quán và khoảng StopOrder trống. Không chỉ áp dụng TripId 6.

## Điều kiện schema

Bản này không thêm migration. `InitialSqlServer`, `AddContactMessages`, `AddTripStops`, model, snapshot và history hiện có được giữ nguyên. Chạy `--check-db` đúng profile, cần Pending migrations 0. Nếu DB chưa có AddTripStops, dùng tài khoản triển khai có quyền DDL để áp **migration tăng thêm còn thiếu**, không yêu cầu web login có db_owner/sysadmin thường trực.

Có thể sinh script trên máy có .NET 9 bằng lệnh dưới, sau khi kiểm tra database đang ở AddContactMessages; quản trị viên xem script rồi chạy trên đúng DB trong SSMS:

```powershell
dotnet ef migrations script 20261102000100_AddContactMessages 20261103000100_AddTripStops --project BusBooking --output Database/AddTripStops_DEPLOY.sql
```

Không chạy đoạn này nếu trạng thái thực khác; không baseline migration chỉ vì thấy bảng tồn tại. Nếu có bảng nhưng thiếu history, cần đối chiếu schema và sao lưu bởi người quản lý DB trước. Bản cập nhật BusGo không cung cấp lệnh ghi giả history.

## Duyệt theo từng chuyến trong Admin

1. Đăng nhập Admin → Chuyến → Điểm đón/trả của đúng chuyến.
2. Lịch hiện tại luôn được hiển thị. Nếu từng có hold, phần sửa/xóa bị khóa.
3. Phần **Bổ sung điểm còn thiếu** hiển thị danh sách có thể thêm và giờ UTC+7 tính từ chuyến. Bấm **Bổ sung các điểm này** để xác nhận.
4. Phần **Thêm một điểm mới** cho nhập tên, địa chỉ (có thể để trống), số phút sau khởi hành, cờ đón/trả. Phải nằm giữa hai điểm đã có và có khoảng thứ tự trống. Có thể dùng cho các tuyến/chuyến sau này mà không sửa code.
5. Khi lịch thay đổi từ một phiên khác, revision cũ bị từ chối. Tải lại trước khi tiếp tục. Khi lịch không phù hợp, thông báo lý do; không có success giả.

## Lệnh xem trước và áp dụng hàng loạt

Dừng web nếu muốn đơn giản hóa đối chiếu báo cáo; bản thân service vẫn dùng gate chung với giữ/xác nhận vé. Sao lưu database trước khi áp dụng.

```powershell
# Không ghi dữ liệu
dotnet run --project BusBooking --launch-profile SqlServer -- --review-dalat-stops
# Sau khi đọc CSV xem trước, áp dụng các chuyến đủ điều kiện
dotnet run --project BusBooking --launch-profile SqlServer -- --complete-dalat-stops
# Chạy lại để kiểm tra không phát sinh trùng
dotnet run --project BusBooking --launch-profile SqlServer -- --complete-dalat-stops
```

Mặc định CSV nằm trong thư mục project `BusBooking/`, tên `dalat-stops-preview.csv` và `dalat-stops-report.csv`; có thể thêm `--stop-report "C:\BaoCao\diem.csv"` (thư mục phải tồn tại và được phép ghi). Trạng thái `planned` chỉ xem trước, StopId 0 là đề xuất chưa INSERT; `applied` đã commit; `complete` đã đủ và không ghi; `blocked` không thay đổi chuyến. Lệnh trả exit 2 khi còn mục bị chặn, kể cả chuyến lịch sử; xem từng lý do. CSV phản ánh DB lúc chạy, không phải báo cáo đã chạy trên máy bạn trong ZIP.

Các lệnh cũ `--update-trip-stops --study-stops` vẫn có: chiều Đà Lạt → TP.HCM được chuyển qua service chỉ-thêm và có báo cáo riêng. Khuyến nghị dùng hai lệnh rõ ràng ở trên cho yêu cầu này. Nhánh các chiều khác giữ guard cũ.

## Bảng cấu hình và nguồn

| Điểm | Offset sau khởi hành | Ví dụ giờ nếu khởi hành 23:00 ngày D |
|---|---:|---|
| Đà Lạt | 0 phút | 23:00 ngày D |
| Đức Trọng | 90 phút | 00:30 ngày D+1 |
| Bảo Lộc | 180 phút | 02:00 ngày D+1 |
| TP. Hồ Chí Minh | 420 phút | 06:00 ngày D+1 |

Bảng là cấu hình học tập đã có trong source `StudyTemplate`, **không phải lịch nhà xe được kiểm chứng, địa chỉ văn phòng BusGo hoặc ETA trực tiếp**. Ví dụ giờ không phải dữ liệu của một TripId thật. Cờ IsIllustrative=true của điểm mới vẫn được giữ nội bộ; không giả địa chỉ bến/văn phòng và không đổi cờ điểm cũ. Tên hiển thị bỏ tiền tố giải thích đã biết nhưng giá trị raw trong DB/snapshot vẫn còn.

Tổng lịch khác đúng 420 phút bị chặn tự bổ sung; không nội suy/đảo lịch chiều đi. Admin cần cấu hình riêng theo thông tin có căn cứ. Chuyến mới do CatalogService tạo, đúng chiều 420 phút, được tạo endpoint và điểm giữa trong cùng transaction; không tự ghi thêm vào chuyến cũ mỗi lần F5.

## Bảo toàn lịch sử và giao dịch

- Transaction ngắn dùng `DbGate.Trip`, cùng khóa với Acquire/Confirm. Chỉ INSERT TripStop; tìm khoảng thứ tự còn trống, không renumber hoặc UPDATE/DELETE điểm cũ.
- Giữ nguyên ID, tên raw, địa chỉ, thứ tự, offset, cờ mọi điểm cũ, trip geometry, tuyến/xe/giờ/giá/phân công. Không ghi SeatHolds, Bookings, Tickets, Payments; không đổi điểm đang chọn hoặc ExpiresAt.
- Chỉ khi thực sự thêm: tăng Revision, audit metadata trong transaction, phát TripChanged sau commit. Không phát lựa chọn riêng của khách lên group công khai.
- Trùng tên với giờ/cờ khác, điểm khác chiếm cùng giờ, lịch sai thứ tự, hết khoảng order, đủ 20 điểm, chuyến đã khởi hành/không Scheduled đều bị chặn; không xóa hold/vé hoặc sửa snapshot để vượt guard.
- Giá vẫn toàn chuyến. Server giữ nguyên kiểm đúng TripId, quyền đón/trả, thứ tự, ownership, idempotency và hạn xác nhận cuối SQL. Nhánh sửa toàn lịch vẫn từ chối nếu có bất kỳ hold nào, kể cả đã hết hạn.

Chưa chạy SQL thực trong môi trường bàn giao; test SQL đã bổ sung trong Tests/BusBooking.SqlTests và chỉ được chạy trên DB riêng `_Tests`. Xem TEST_RESULTS.md.
