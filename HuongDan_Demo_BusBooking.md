# Hướng dẫn demo và chụp minh chứng BusBooking

## Chuẩn bị

Dùng một database kiểm thử riêng và hai tài khoản khách hàng khác nhau. Mở một cửa sổ Edge thường và một cửa sổ InPrivate để tách phiên đăng nhập. Chọn chuyến tương lai còn mở bán. Ghi lại mã chuyến, mã ghế và phiên bản ứng dụng đang chạy.

Mở hai cửa sổ Developer PowerShell tại thư mục chứa `BusBooking.sln`. Trong cửa sổ thứ nhất, tạo chuyến và tài khoản kiểm thử.

```powershell
Set-Location -LiteralPath 'C:\Users\Admin\Downloads\BusCenter_CONTACT_STOPS_BANNER_REVIEWED\BusBooking'
$env:SQL_TEST_CONNECTION = 'Server=MSI\SQLEXPRESS;Database=BusCenterPython_Tests;Integrated Security=True;TrustServerCertificate=True'
dotnet run --project Tests/BusBooking.SqlTests -- --ack-test-db --fixtures-only
```

Lệnh in đường dẫn tới `fixtures.json`. Trong cửa sổ thứ hai, chạy web kiểm thử trên cùng database.

```powershell
Set-Location -LiteralPath 'C:\Users\Admin\Downloads\BusCenter_CONTACT_STOPS_BANNER_REVIEWED\BusBooking'
$env:ConnectionStrings__DefaultConnection = 'Server=MSI\SQLEXPRESS;Database=BusCenterPython_Tests;Integrated Security=True;TrustServerCertificate=True'
$env:DemoPayment__Bank = 'VietinBank'
$env:DemoPayment__BankBin = '970415'
$env:DemoPayment__Account = '0000000000'
dotnet run --project BusBooking --launch-profile SqlServer -- --urls http://127.0.0.1:5199
```

Nếu cổng 5199 đang dùng, chọn một cổng trống và dùng cùng URL trong lệnh Python. Tài khoản kiểm thử nằm trong `fixtures.json`. Giữ file này riêng khi nộp báo cáo.

## Chạy Python để lấy số liệu mục 3.6

Trong cửa sổ thứ nhất, điền đúng đường dẫn fixture đã in.

```powershell
python Tests/demo_giu_ghe.py --fixture '.\TestResults\<ma-lan-chay>\fixtures.json' --url http://127.0.0.1:5199 --sql-server 'MSI\SQLEXPRESS' --confirm --wait-300
```

Script chạy lần lượt 20, 50 và 100 yêu cầu. Mỗi đợt dùng một ghế riêng. Tùy chọn `--confirm` xác nhận và hoàn tất QR mô phỏng cho người thắng ở đợt cuối. Tùy chọn `--wait-300` quan sát worker nhả một ghế khác sau đủ năm phút. Chờ đến khi có kết quả cuối.

- Lấy số HTTP 200, 409, lỗi khác, P95 và vé trùng từ `tong-hop.csv`. Giữ `yeu-cau.csv` làm log từng yêu cầu.
- Lấy hạn SQL, thời gian quan sát, HTTP 409 của mã cũ và HTTP 200 của khách giữ lại từ `het-han.json`.
- Điền số liệu quan sát vào Bảng 3.6. Mỗi đợt kỳ vọng một HTTP 200, các yêu cầu còn lại trả 409 và số vé trùng bằng 0 khi đối chiếu SQL.
- Nếu báo lỗi SSPI, kiểm tra kết nối Windows Authentication đến đúng instance từ Developer PowerShell. Giữ trạng thái “chưa đo” trong báo cáo cho đến khi có kết quả.

## Danh sách ảnh và vị trí chèn

| Ảnh cần chụp | Thao tác và nội dung cần thấy | Chèn vào |
|---|---|---|
| Ghế theo từng chuyến | Mở hai chuyến khác nhau dùng cùng xe, nếu lịch mẫu có. Chụp mã chuyến, biển số và cùng mã ghế A01. Giữ A01 ở chuyến thứ nhất rồi cho thấy A01 ở chuyến kia vẫn trống. | Mục 2.4, ngay sau Sơ đồ 2.4. |
| Hai khách theo dõi cùng chuyến | Đặt hai cửa sổ cạnh nhau. Khách A giữ một ghế, màn A có bộ đếm và màn B cập nhật ghế sang Đang giữ. Để mã chuyến và mã ghế cùng xuất hiện trong ảnh. | Mục 3.3 hoặc ca CN06 ở mục 3.6. |
| QR mô phỏng | Khách A xác nhận thông tin, chụp màn QR cùng hạn thanh toán. Che số tài khoản và mã QR nếu đang sử dụng cấu hình có dữ liệu ngân hàng thật. | Mục 3.3, trước ảnh xác nhận thành công. |
| Vé sau thanh toán | Bấm Hoàn tất, chụp thông báo Đặt vé thành công hoặc trang chi tiết vé có trạng thái Đã thanh toán và mã check-in. Che số điện thoại và mã có thể dùng để check-in trước khi nộp. | Mục 3.3. |
| Hết hạn và nhả ghế | Dùng ghế khác, chụp lúc mới giữ và sau khi đợi đủ 5 phút. Chụp ghế trống và kết quả `het-han.json`. | Mục 3.6, ca CN03. |
| Tranh chấp đồng thời | Chụp màn hình terminal có đủ ba đợt, mở CSV để đối chiếu. | Mục 3.6, sau Bảng 3.6 hoặc đưa log dài vào phụ lục. |
| Mất SignalR | Trong DevTools của khách B, chặn mẫu `*/hubs/seats*` rồi tải lại. Giữ HTTP hoạt động. A giữ ghế, B thử giữ khi sơ đồ còn cũ. Chụp phản hồi 409, sau đó bỏ chặn và đồng bộ lại. | Mục 3.6, ca CN06. |

Nếu khó thấy xung đột trong ca mất SignalR vì giao diện đã đồng bộ định kỳ, cho B thao tác ngay sau A hoặc dùng kết quả HTTP của bài thử đồng thời để minh chứng riêng việc SQL từ chối yêu cầu trùng.

Trên Word, đặt ảnh cùng chú thích gần đoạn mô tả tương ứng. Dùng chú thích dạng “Hình 3.x. Khách hàng A giữ ghế trên chuyến thử nghiệm” và ghi “Nguồn: ảnh chụp từ hệ thống thực nghiệm Bus Center”. Cập nhật danh mục hình và mục lục sau khi chèn ảnh.
