# Kế hoạch hoàn thiện đồ án Bus Center

**Đề tài** — Xây dựng hệ thống quản lý xe khách và đặt vé trực tuyến tích hợp cơ chế giữ chỗ theo thời gian thực.

**Nền tảng** — ASP.NET Core MVC, Razor, EF Core, SQL Server, Identity và SignalR trong solution `BusBooking.sln`. Bản Stitch được dùng làm mẫu thị giác cho tìm chuyến, sơ đồ ghế, xác nhận và trung tâm điều hành. Giao diện xác nhận theo nghiệp vụ thu trực tiếp hiện tại.

## 1. Các màn hình và thứ tự ưu tiên

| Mức | Vai trò | Màn hình | Việc cần hoàn thiện và tiêu chí đạt |
| --- | --- | --- | --- |
| P0 | Khách vãng lai | Trang chủ, tìm chuyến, đăng ký/đăng nhập, chính sách, liên hệ | Tìm theo tuyến/ngày Việt Nam. Có trạng thái trống, lỗi và đường quay lại đúng chuyến sau đăng nhập. |
| P0 | Khách hàng | Chi tiết chuyến, sơ đồ ghế, điểm đón/trả | Phân biệt ghế trống, đang chọn, đang giữ, giữ của tôi và đã đặt bằng màu kèm nhãn. Hai trình duyệt nhìn thấy kết quả sau giao dịch. |
| P0 | Khách hàng | Thông tin hành khách và xác nhận vé | Hiện thời gian giữ còn lại, điểm đón/trả, tổng giá và hình thức thu. Quá hạn thì đồng bộ ghế. Gửi lại cùng thao tác chỉ nhận một kết quả. |
| P0 | Khách hàng | Vé của tôi, chi tiết vé, hồ sơ, yêu cầu đổi/hủy | Có mã vé, trạng thái, lịch sử thu/hoàn và hỗ trợ đổi/hủy. Tài khoản khác truy cập chi tiết phải bị từ chối. |
| P0 | Nhân viên | Phòng vé, danh sách khách, yêu cầu đổi/hủy, thu/hoàn | Tìm theo mã vé hoặc chuyến. Hiện nội dung xác nhận trước tác vụ. Đổi vé giữ được vé cũ nếu giao dịch thay thế thất bại. |
| P0 | Tài xế | Lịch được giao, danh sách khách, tiến trình chuyến | Chỉ hiện chuyến được phân công. Kiểm tra tuần tự chuẩn bị chạy, đang chạy, hoàn thành. |
| P0 | Quản trị | Tuyến, điểm đón/trả, sơ đồ xe, phương tiện, chuyến, tài khoản, liên hệ, audit | Chặn lịch xe/tài xế chồng nhau. Bảo toàn mã ghế và địa điểm đã được ghi trên vé cũ. Dữ liệu vận hành do nhà xe duyệt. |
| P1 | Quản trị và nhân viên | Giám sát giữ chỗ, bộ lọc và phân trang, báo cáo theo tuyến/chuyến | Thêm tỷ lệ lấp đầy và thời gian truyền cập nhật dựa trên số liệu đo. Tách vé đã tạo, tiền đã thu, tiền đã hoàn và tiền chờ hoàn. |
| P1 | Khách hàng và nhân viên | Vé in, QR tra cứu, check-in | QR dùng token có thể thu hồi. Quét nhiều lần cho cùng một vé chỉ ghi một lượt check-in. Không đưa tên hoặc số điện thoại vào QR. |
| P2 | Khách hàng và kế toán | Thanh toán online, kết quả chờ/thành công/thất bại, đối soát | Chỉ tích hợp khi đã chốt cổng và quy tắc tiền đến sau khi hết giữ chỗ. Dùng môi trường thử của nhà cung cấp trước. |

Các màn hình P0 phần lớn đã có trong source. Công việc P0 tập trung vào kiểm chứng liên vai trò, luồng lỗi và hoàn thiện dữ liệu vận hành. P1 và P2 là các hạng mục phát triển tiếp.

## 2. SQL Server và cập nhật thời gian thực

1. **Chốt đường cơ sở.** Chạy profile SQL đang dùng với `--check-db`. Kiểm tra lịch sử migration, tính tương thích model EF, bản sao lưu và khả năng khôi phục trên DB thử. Giữ tên biến cấu hình và User Secrets hiện tại. Đối chiếu các chuyến còn bán với điểm đón/trả, sơ đồ ghế và nhân sự được phân công.
2. **Chứng minh giữ ghế nhất quán.** Kiểm tra ràng buộc duy nhất ghế theo chuyến, một booking cho mỗi hold và chỉ một ticket còn hiệu lực cho một ghế. Xác minh toàn bộ thao tác giữ nhiều ghế, xác nhận, nhả, hết hạn và đổi vé nằm trong giao dịch phù hợp. Dùng giờ UTC cho hạn giữ và giờ Việt Nam khi hiển thị. Duy trì request key để thử lại sau timeout mà không đặt vé hai lần.
3. **Kiểm chứng phục hồi SignalR.** Ghi nhận thời gian từ commit đến client nhận revision, thử mất mạng và khởi động lại tiến trình. Khi kết nối lại, tải snapshot và bỏ qua revision cũ. Nếu triển khai nhiều máy và cần phát lại sự kiện, thiết kế bảng outbox cùng transaction và thử consumer nhận trùng trước khi triển khai.
4. **Kiểm tra truy vấn vận hành.** Đo execution plan và logical reads cho tìm chuyến, snapshot ghế, lịch sử vé, danh sách xử lý hết hạn và sổ thu hoàn. Chỉ thêm index khi có số đo. Khi phân trang, dùng thứ tự ổn định theo thời gian và khóa chính. Báo cáo tiền lấy từ bút toán thực tế.
5. **Mở rộng có migration riêng.** Check-in thêm bảng lượt lên xe với khóa duy nhất theo vé. Thanh toán online thêm bảng lần thử và sự kiện callback với mã giao dịch duy nhất, xác thực chữ ký và lưu trạng thái đối soát. Mỗi migration cần script duyệt trước, sao lưu, thử trên bản sao dữ liệu và phương án khôi phục đã diễn tập.

## 3. Kịch bản nghiệm thu và bảo vệ

| Đợt | Việc làm | Bằng chứng cần có |
| --- | --- | --- |
| 1 | Chạy đủ bốn vai trò trên DB thử. Kiểm giao diện ở 390, 768, 1440 px và dùng bàn phím. | Build, bảng quyền, ảnh thực tế, lỗi HTTP và JavaScript. |
| 2 | Hai phiên tranh cùng một ghế, giữ nhiều ghế có một ghế xung đột, gửi lại cùng request key. | Một giao dịch thắng, nhóm ghế không bị giữ dở, chỉ một vé hiệu lực cho mỗi ghế. |
| 3 | Chờ hết hạn 300 giây, ngắt mạng và nối lại, khởi động lại worker, xác nhận gần hạn. | Dấu thời gian UTC, revision SignalR và ảnh hai phiên trước/sau khi đồng bộ. |
| 4 | Chạy kịch bản tải 20/50/100 client, ghi số lần xung đột, lỗi, P50/P95 và độ trễ phát sự kiện. | Số đo thực, cấu hình máy và dữ liệu thử đủ để chạy lại. |
| 5 | Demo tìm chuyến → giữ ghế → xác nhận → nhân viên ghi nhận thu → tài xế xem khách. | ERD, sơ đồ trạng thái, trình tự giao dịch, bảng quyền và kịch bản demo khoảng 5–7 phút. |

Ưu tiên hoàn tất đợt 1–3 trước khi thêm QR hoặc thanh toán. Đây là ba đợt trực tiếp chứng minh phần giữ ghế theo thời gian thực của đề tài.
