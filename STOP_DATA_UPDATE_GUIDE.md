# Cập nhật điểm cụ thể cho SQL Server hiện có

## Trước khi ghi

Dừng web nếu muốn đối chiếu báo cáo đơn giản; sao lưu DB/source. Giữ kết nối SQL đang hoạt động, không seed/reset/baseline. Bản này không đổi schema; kiểm --check-db đúng profile, cần Pending migrations0. Nếu thiếu migration cũ, dùng tài khoản triển khai áp đúng phần tăng thêm; tài khoản web chỉ cần quyền đọc/ghi TripStops/Trips/AuditEntries như chức năng hiện có, không cần db_owner thường trực.

Đọc STOP_SOURCE_REVIEW: các bến công khai mới là ứng viên. Không tự nhận văn phòng/quyền khai thác từ tên bến hoặc template. Cần tên điểm cụ thể, địa chỉ đã đối chiếu, khu vực phục vụ, cờ đón/trả và xác nhận lịch/quyền khai thác của chính chuyến.

## Cách thuận tiện: Admin

1. Đăng nhập Admin → Chuyến → Điểm đón/trả của đúng chuyến Scheduled chưa đến giờ đi.
2. Trong **Thêm địa điểm đón/trả cụ thể**, nhập tên bến/quầy/điểm hẹn (không chỉ tên thành phố), địa chỉ, khu vực, phút sau khởi hành, chọn cờ phù hợp và ghi nguồn/căn cứ.
3. Điểm đầu offset0: khu vực phải khớp nơi đi, chỉ Đón. Điểm cuối offset bằng tổng phút chuyến: khu vực khớp nơi đến, chỉ Trả. Giờ hiển thị UTC+7 theo chuyến hiện có. Không đổi DepartureUtc/ArrivalUtc để khớp website hãng khác.
4. Điểm giữa không tự mở hai cờ; chỉ chọn thao tác đã được xác nhận, offset riêng đúng chiều. Đánh dấu xác nhận địa điểm/giờ/quyền khai thác trước khi lưu.
5. Màn hình hiển thị giờ theo input và yêu cầu xác nhận; service kiểm lại revision/điều kiện trong transaction. Kết quả lỗi/xung đột giữ dữ liệu cũ, không dựng success. Sau khi thêm, lịch điểm hiện tại có ID mới thật từ SQL.

Khi thêm hai đầu ở hai lần riêng, lần đầu chỉ thay lựa chọn mới ở đầu đã duyệt; đầu chưa duyệt vẫn là fallback cũ. Muốn công bố hai đầu trong cùng transaction, dùng kế hoạch batch bên dưới. Link “Liên hệ xác nhận địa điểm cụ thể” hiện khi điểm đang chọn chưa đủ tên/địa chỉ; không tuyên bố đã có bến thật ở các chuyến chưa được duyệt.

Full editor cũ vẫn khóa nếu có bất kỳ hold lịch sử nào. Khi chưa có hold và muốn sửa/thu gọn toàn lịch, dùng full editor và loại các dòng không dùng, giữ offset tăng; không dùng xóa hold/vé để mở khóa. Công cụ legacy được đặt trong mục mở rộng, không phải nơi phê duyệt địa điểm mới.

## Batch có dry-run/báo cáo

`Documentation/SPECIFIC_STOP_PLAN_TEMPLATE.json` cố ý chưa chạy được (TripId0, tên/địa chỉ/căn cứ trống, xác nhậnfalse). Sao chép ra tệp riêng, điền TripId/Revision hiện tại và Points đã duyệt. Không hard-code TripId6 hoặc ngày; mỗi mục chọn đúng chuyến thực. Không đổi Revision thành số mới chỉ để lách báo stale: mở lại lịch và duyệt lại trước.

Các trường: TripId,Revision,Points[] với Name,Address,OffsetMinutes,CanPickup,CanDropoff,Area,SourceReference,OperatorConfirmed. SourceReference tối đa500 ký tự, Area160. Không ghi PII, password hoặc thông tin khách vào kế hoạch.

```powershell
# Thay đường dẫn bằng kế hoạch đã điền và duyệt của bạn
dotnet run --project BusBooking --launch-profile SqlServer -- --review-stop-plan "C:\BaoCao\ke-hoach-diem.json"
# Đọc BusBooking/specific-stop-preview.json; sau đó mới áp dụng
dotnet run --project BusBooking --launch-profile SqlServer -- --apply-stop-plan "C:\BaoCao\ke-hoach-diem.json"
```

Có thể thêm --stop-report với đường dẫn tệp báo cáo trong thư mục có quyền ghi. JSON không quá2MB, tối đa1000 chuyến, không lặp TripId; field không biết bị từ chối. Mỗi chuyến là transaction riêng, toàn bộ Points của chuyến đó cùng thành công hoặc không ghi. `planned` chưa INSERT (Id0); `applied` đã commit; `complete` đã đủ không INSERT/revision; `blocked` không ghi và có lý do. Exit2 nghĩa có mục cần kiểm tra, không phải mọi chuyến đã cập nhật. Lỗi SQL bất ngờ lưu báo cáo các commit trước rồi dừng; không chạy lại không xem báo cáo.

Chạy lặp cùng nội dung với revision hiện tại không tạo trùng. Kế hoạch mang revision cũ sau lần INSERT bị từ chối stale: đây là bảo vệ, không phải chạy lại dữ liệu. Sau khi đối chiếu, xuất kế hoạch với revision mới để kiểm `complete` và không tăng thêm revision/audit. Activehold/confirm có thể tăng revision; phải tải/duyệt lại nếu xảy ra giữa xem trước và áp dụng.

Báo cáo ghi before/after từ SQL, geometryHash (không có Revision), ID/tên/địa chỉ/order/offset/cờ điểm và đề xuất. Hai lượt đọc báo cáo nằm ngoài transaction ghi ngắn, nên thay đổi hợp lệ đồng thời có thể xuất hiện giữa chúng; không lấy hash khác làm bằng chứng service tự đổi lịch hoặc lấy hash giống làm test concurrency. Không đưa báo cáo DB cá nhân vào ZIP/chia sẻ nếu có địa chỉ riêng.

## Bảo vệ dữ liệu và lựa chọn

- Dùng đúng DbGate.Trip chung Acquire/Confirm, transaction ngắn. Chỉ INSERT điểm; giữ mọi ID/order/offset/name/address/flags cũ. Endpoint thay thế dùng order trống trước đầu/sau cuối, offset vẫn0/duration; schema int order hiện có cho phép, không renumber. Giữa đường chèn vào gap an toàn; không còn gap/đủ20điểm thì báo chặn.
- Không viết SeatHold/Booking/Ticket/Payment hoặc thay geometry/fare/phân công/status. Chỉ tăng Trip.Revision khi thực sự thêm, audit trong transaction; nguồn được lưu riêng thành AuditEntry500ký tự, không tràn cột. TripChanged chỉ sau commit, không phát lựa chọn riêng của khách.
- Source/cờ schedule configured được giữ nội bộ; không đặt IsIllustrative=false để giả xác minh. Không ghi đè điểm custom cùng tên có địa chỉ/giờ/cờ khác.
- Lượt đặt mới không chọn các địa phương mẫu giữa đường. Khi có endpoint cụ thể cùng đầu, endpoint chung cũ không còn được đề nghị cho lượt mới. Mặc định đón ở offset0/trả ở duration; điểm giữa chỉ chọn rõ với cờ đã cấu hình, không thành mặc định ngẫu nhiên.
- Snapshot riêng của khách còn giữ các ID đã chọn trong hold của chính họ, đánh dấu CanSelect=false nếu chỉ còn để bảo toàn cam kết. UI không đưa điểm cũ của holdA sang lựa chọn holdB. Server ResolveHeld chỉ cho dùng lại điểm cũ đúng vai trò đã lưu; chọn điểm mới phải qua policy. Confirm lưu raw snapshot như cũ, không rewrite vé lịch sử.
- Giữ đúng TripId/active/pickup/drop/order/time; lựa chọn trả phải sau đón. Không đổi giá toàn chuyến, TTL300s, idempotency/ownership/expiry/finalSQL. Không chia segment inventory.

Chuyến mới vẫn được tạo như trước; vào Admin của chuyến mới để duyệt địa điểm thật hoặc dùng batch mới. Endpoint chung chưa duyệt được giữ vì chưa có dữ liệu/quyền khai thác Bus Center, không tự lấy FUTA làm mặc định. Legacy --complete-dalat-stops vẫn có nhưng chỉ bổ sung địa phương mẫu nội bộ; không thay cho quy trình duyệt trên.

Runtime SQL/idempotency/concurrency chưa chạy trong môi trường bàn giao. Các test mới và phần chưa xác minh ở TEST_RESULTS.md; không có báo cáo DB TripId thật của bạn trong ZIP.
