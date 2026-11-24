# Điểm đón/trả theo chuyến — cập nhật 03/11/2026

Giữ nguyên đề tài chuyên ngành Thương mại điện tử, .NET 9/SQL Server và giá/ghế theo toàn chuyến. Không bán lại một ghế trên các chặng khác nhau. Không đổi lịch xe để khớp ảnh tham khảo FUTA.

## Cập nhật database đang dùng

1. Stop Debugging trong Visual Studio, backup database hiện tại bằng SSMS → Tasks → Back Up.
2. Giải nén ZIP mới sang thư mục riêng; mở `BusBooking.sln`. Giữ connection string của máy bạn qua user-secrets như trước (UserSecretsId của project không đổi). Không gửi connection string/mật khẩu lên chat.
3. Terminal tại thư mục solution:

```powershell
dotnet tool restore
dotnet restore BusBooking.sln
dotnet build BusBooking.sln
dotnet ef migrations list --project BusBooking
dotnet ef migrations has-pending-model-changes --project BusBooking
```

Nếu `InitialSqlServer` vẫn Pending nhưng database đã có các bảng cũ: **dừng**, giữ nguyên history/baseline đã thiết lập trước đây; không chạy lại Initial, không xóa DB/history. Kiểm đúng connection string và hướng dẫn baseline của bản trước. Nếu có pending model changes, dừng để kiểm model/migration, không tắt cảnh báo.

Sau khi history/model đúng, cập nhật các migration còn thiếu (bao gồm Contacts nếu trước đó chưa áp):

```powershell
dotnet ef database update --project BusBooking
dotnet run --project BusBooking --launch-profile SqlServer -- --update-trip-stops --study-stops
dotnet run --project BusBooking --launch-profile SqlServer
```

Migration mới `20261103000100_AddTripStops` thêm schema và hai điểm đầu/cuối cho **mọi chuyến đã có**, không update dòng Trips, ghế, vé, Identity hoặc Payments. Không chạy lại seed cũ để có điểm dừng. Startup bình thường không tự migrate/seed/reset.

Lệnh `--update-trip-stops` là thao tác có chủ đích, mỗi chuyến một transaction ngắn với trip gate. Thiếu lịch thì thêm endpoint; không ghi đè lịch tùy chỉnh. `--study-stops` chỉ bổ sung ví dụ trung gian khi tên tuyến, mô tả học tập, biển TEST, tổng thời gian và lịch mặc định khớp chính xác, chuyến Scheduled và **chưa có bất kỳ lịch sử hold nào**. Chạy lại không trùng. Chuyến đã có giữ/vé tiếp tục dùng hai endpoint, lịch cũ được bảo vệ. Không xóa hold để mở khóa.

Báo cáo thật của database máy bạn được tạo sau khi chạy lệnh tại `BusBooking/trip-stops-report.csv`. Có TripId, tuyến, DepartureUtc, ArrivalUtc, Fare, tên/thứ tự/giờ điểm, cờ minh họa và hành động. Bản bàn giao không có DB máy bạn nên không cung cấp bảng TripId giả. Bỏ `--study-stops` nếu chỉ muốn endpoint trung tính.

## Lịch mô phỏng được đóng gói

Các mốc dưới là quyết định của bộ dữ liệu học tập, **không phải giờ xe thực tế/ước tính giao thông**, không phải lịch của FUTA, không phải địa chỉ văn phòng vận tải. Không nội suy cho chuyến có tổng thời gian khác.

| Tuyến, tổng thời gian khớp | Điểm học tập và phút từ khởi hành |
|---|---|
| TP. Hồ Chí Minh → Đà Lạt, 420 phút | Bảo Lộc 240; Đức Trọng 330 |
| Đà Lạt → TP. Hồ Chí Minh, 420 phút | Đức Trọng 90; Bảo Lộc 180 |
| TP. Hồ Chí Minh → Nha Trang, 480 phút | Cam Ranh 330; Cam Lâm 400 |

Không dùng địa chỉ công khai chưa xác minh; trường địa chỉ để trống. Tuyến khác có điểm đầu/cuối theo tên tuyến; quản trị viên tự cấu hình lịch đáng tin qua giao diện.

## Cấu hình lịch, dữ liệu và vé

Admin → Chuyến & phân công → **Điểm đón / trả**. Chèn điểm trước điểm cuối, đặt tên, phút từ khởi hành, cờ đón/trả/hoạt động/lịch học tập rồi lưu. Từ 2–20 điểm, offset tăng nghiêm ngặt, endpoint 0 và tổng phút của chuyến. Màn hình không thay giờ/xe/giá/phân công. Sau khi chuyến có lịch sử hold, lịch điểm bị khóa để giữ cam kết với khách; không có xóa cascade. Sửa danh mục/chuyến còn phải tuân thủ ràng buộc cũ.

`TripStops`: TripId + StopOrder unique, offset phút, tên/địa chỉ, cờ đón/trả/hoạt động/minh họa. Offset được tính từ UTC khởi hành; endpoint cuối trả về ArrivalUtc chính xác (kể cả giây), không sửa ArrivalUtc. Lưu UTC, hiển thị UTC+7, 24 giờ và ngày đầy đủ. Chuỗi offset nhập theo phút; giờ kế bên bảng là dữ liệu đã lưu/đã submit, không tự chạy đồng hồ giả.

Acquire giữ ID điểm vào SeatHold. `POST /api/seats/stops` cho chủ lượt giữ đổi điểm trong hạn, không tạo hold/không tăng ExpiresAt. Confirm kiểm điểm thuộc đúng chuyến, hoạt động/cờ/thứ tự/giờ và lưu ID cùng snapshot tên/địa chỉ/giờ vào Booking. Mỗi đơn dùng một cặp điểm. ID không gửi từ client cũ dùng cặp endpoint hợp lệ mặc định; client mới luôn gửi lựa chọn rõ ràng. Không tin giá/timestamp do client gửi.

Vé cũ có các cột mới null, hiển thị “Chưa ghi nhận điểm cụ thể”; không gán lựa chọn giả cho khách cũ. Snapshot trên vé không đổi theo danh mục. Retry confirm không tạo vé thứ hai; nếu gửi điểm khác booking đã xác nhận, trả conflict thay vì ghi đè. Đổi vé chọn điểm thuộc chuyến mới, thất bại rollback giữ nguyên vé cũ.

`Booking:ArriveEarlyMinutes` mặc định 15: quy định học tập hiển thị giờ phải có mặt = giờ đón − 15 phút. Không phải thời gian khởi hành. Giờ bán/đổi/hủy tiếp tục căn cứ toàn chuyến; không tự mở bán sau giờ khởi hành vì khách chọn điểm giữa.

## Trải nghiệm đặt vé

Chọn ghế/điểm → **Đặt vé** (Acquire 300 giây) → form hành khách + countdown → **Xác nhận đặt vé**. Chưa thu tiền vẫn hiển thị chưa thu như trước. Nút Tiếp tục đưa về form khi có hold. Tóm tắt dùng ghế của activeHold, không mất total khi mảng selected đã clear. Refresh khôi phục điểm từ snapshot riêng tư. Chuyển hold có draft hành khách riêng trong bộ nhớ trang; reload không lưu thông tin hành khách vào localStorage.

## Trạng thái xác minh

DOM/HTTP fixture JavaScript đã chạy, xem TEST_RESULTS. EF migration/SQL, HTTP MVC và lịch DB máy bạn **CHƯA KIỂM CHỨNG** trong môi trường bàn giao. Chạy kiểm tra trước khi dùng để bảo vệ đồ án.
