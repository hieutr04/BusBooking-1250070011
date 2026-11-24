# Bus Center — CONTACT_STOPS_BANNER_REVIEWED, 03/11/2026

## Bản bổ sung giám sát giữ chỗ và QR lên xe

- Admin mở **Giám sát giữ chỗ** rồi chọn chuyến. Sơ đồ nhận cập nhật SignalR và tự lấy lại dữ liệu theo định kỳ khi mất kết nối. Số lượt ghế hết hạn tính theo lịch sử của chuyến, nên cùng một ghế có thể được tính nhiều lần nếu từng được giữ rồi hết hạn nhiều lần.
- Khách mở **Chi tiết vé → Vé điện tử lên xe**. Mỗi ghế có một mã check-in. Trong danh sách hành khách, Admin hoặc nhân viên/tài xế được phân công có thể quét bằng camera, chọn ảnh QR hoặc nhập mã. Check-in mở khi chuyến ở trạng thái chuẩn bị chạy hoặc đang chạy.
- Bản này thêm migration `20261104000100_AddTicketCheckIn` với cột thời gian check-in cho vé. Sau khi dừng Visual Studio và sao lưu database, chạy tại thư mục chứa solution:

```powershell
dotnet tool restore
dotnet ef database update --project BusBooking
```

Lệnh lấy cấu hình SQL từ User Secrets hoặc biến môi trường hiện tại. Camera yêu cầu quyền truy cập của trình duyệt và trang chạy qua localhost hoặc HTTPS.

Sửa trực tiếp ZIP BusGo_UI_CLEANUP_STOPS_REVIEWED(2) đính kèm. Giữ nguyên đề tài **XÂY DỰNG HỆ THỐNG QUẢN LÝ XE KHÁCH VÀ ĐẶT VÉ TRỰC TUYẾN TÍCH HỢP CƠ CHẾ GIỮ CHỖ THEO THỜI GIAN THỰC**, chuyên ngành **Thương mại điện tử**, .NET9/SQL Server/EF Core/Identity/SignalR, mọi chức năng và lịch sử đang có.

## Mở và chạy

1. Dừng web (Shift + F5), sao lưu source/database hiện dùng. Giải nén, mở `BusBooking/BusBooking.sln` bằng Visual Studio có SDK9.0.305 tương thích global.json.
2. Giữ User Secrets/cấu hình SQL Server đã chạy thành công trên máy bạn. Không đổi sang LocalDB, không reset DB/tài khoản, không chép mật khẩu vào source. Tên kỹ thuật BusBooking, namespace, UserSecretsId, cookie, role, route và mã vé không đổi.
3. Tại thư mục chứa `.sln`:

```powershell
dotnet run --project BusBooking --launch-profile SqlServer -- --check-db
dotnet run --project BusBooking --launch-profile SqlServer
```

Hoặc chọn profile **SqlServer** rồi F5. Nếu bạn đang dùng Https, dùng thống nhất Https cho kiểm tra/chạy/cập nhật. Giữ các giá trị môi trường đã thiết lập; launchSettings có thể ghi đè User Secrets.

Bản này **không thêm migration/đổi schema/connection string**. Cần Pending migrations:0. Nếu còn migration từ phiên bản trước, xử lý đúng migration tăng thêm bằng tài khoản triển khai trước; không seed lại/ghi giả baseline/chạy Initial chồng bảng.

## Năm thay đổi

1. Brand hiển thị **Bus Center** trên public/workspace/tab/footer/auth/contact/error; logo BC tự vẽ. Không đổi tên kỹ thuật.
2. Liên hệ hai cột/một cột, khối kênh hỗ trợ dùng cùng cấu hình với footer. Form SQL, anti-forgery, PRG, token chống trùng, giới hạn gửi và quyền Admin giữ nguyên.
3. Điểm đón/trả: không đưa địa phương trung gian mẫu vào lượt chọn mới chỉ vì cờ cũ cùng true. Admin có luồng bổ sung địa điểm cụ thể, địa chỉ/khu vực/căn cứ/xác nhận khai thác, hỗ trợ DB hiện có và endpoint thay thế mà không sửa điểm/hold/vé cũ. Khách đang giữ giữ nguyên lựa chọn hợp lệ.
4. Bỏ toàn bộ caption nguồn/tác giả dưới ảnh giao dịch; ghi công tập trung công khai tại **Nguồn ảnh**, có link footer. Giữ ảnh WebP/tác giả/giấy phép, không xóa watermark.
5. Hero hai cột trắng–xanh, ảnh ngang có kích thước dự phòng, form GET tìm chuyến sát hero; ảnh hiện cả mobile, không carousel/khuyến mại giả.

## Thông tin còn cần chủ website duyệt

Chưa có số điện thoại/email/địa chỉ/website/giờ hỗ trợ/pháp nhân thật của Bus Center. Source giữ trống, form là kênh hoạt động; điền theo `CONTACT_CONFIGURATION.md`, không lấy dữ liệu FUTA làm của Bus Center.

Đã khảo sát địa điểm công khai cho3 chiều tuyến nhưng **không có xác nhận Bus Center được khai thác các bến hoặc lịch vận hành thật**. Không tự gán bến của hãng khác vào DB. Xem bảng/nguồn ở `STOP_SOURCE_REVIEW.md` và quy trình `STOP_DATA_UPDATE_GUIDE.md`. Khi chưa duyệt địa điểm cụ thể, endpoint chung cũ được giữ để tương thích và UI có link xác nhận; đây là phần chưa hoàn thiện dữ liệu vận hành, không được nhận là đã sửa đúng mọi chuyến.

Admin → Chuyến → Điểm đón/trả → **Thêm địa điểm đón/trả cụ thể**. Sau khi có thông tin hợp lệ, service INSERT điểm mới trong SQL thật, tăng Revision/audit và phát đồng bộ sau commit. Điểm cụ thể thay thế endpoint chung cho lượt đặt mới; hold/vé cũ không đổi. Không tự ghi DB khi F5.

## Kiểm chứng

Test DOM sản phẩm đã chạy, source/cú pháp/asset/protected-file checks đạt; chi tiết `TEST_RESULTS.md`. Restore/build/C# planner bị chặn CoreCLR0x8007000E, exit137; SQL, Razor/DI/HTTP/browser và screenshots thực **CHƯA KIỂM CHỨNG**. Không có screenshot Home/Contact/Trips chạy MVC thật và không tuyên bố không500. ZIP mang nhãn REVIEWED.

Prompt gốc lần này ở `Documentation/INPUT_BUSCENTER_REQUIREMENTS_ORIGINAL.txt`; mọi tài liệu yêu cầu gốc cũ được giữ. Hướng dẫn/báo cáo trước Bus Center nằm trong `Documentation/history-before-buscenter/` cùng lịch sử đã có; không coi chúng là kết quả lần này.
