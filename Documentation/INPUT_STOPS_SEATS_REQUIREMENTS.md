# BỔ SUNG ĐIỂM ĐÓN/TRẢ, ĐỔI NHÃN “ĐẶT VÉ” VÀ NÂNG CẤP SƠ ĐỒ GHẾ BUSBOOKING

## 1. Giữ nguyên đề tài, công nghệ và phạm vi đã có

Tôi đang thực hiện đồ án chuyên ngành **Thương mại điện tử**, tên đề tài chính thức:

**XÂY DỰNG HỆ THỐNG QUẢN LÝ XE KHÁCH VÀ ĐẶT VÉ TRỰC TUYẾN TÍCH HỢP CƠ CHẾ GIỮ CHỖ THEO THỜI GIAN THỰC**

Source hiện tại đính kèm:

**`QuanLyXeKhach_DatVe_GiuCho_REALTIME_IMAGES_CONTACT_REVIEWED(1).zip`**

Tôi gửi thêm ảnh website hiện tại và ảnh tham khảo FUTA về điểm đón/trả, giờ đón/trả, sơ đồ ghế tầng dưới/tầng trên.

**Hãy sửa trực tiếp source ZIP này. Giữ nguyên tên đề tài, chuyên ngành, .NET 9, SQL Server, nghiệp vụ và tất cả yêu cầu còn lại. Không tạo lại project, không viết lại backend, không redesign toàn bộ website.**

SQL Server trên máy tôi đã kết nối được. Giữ solution/project BusBooking, thương hiệu véxe., gam trắng–xanh, ảnh local và trang Liên hệ đã thêm, Identity và bốn vai trò Admin/Staff/Driver/Customer, các chức năng quản trị, phòng vé, tài xế và khách hàng đang có.

Lần này chỉ thay đổi:

1. Trên trang chọn ghế/đặt vé, thêm **chọn điểm đón và điểm trả** theo chuyến, có giờ đúng với điểm được chọn, tham khảo ảnh FUTA.
2. Đổi chữ trên nút **“Giữ ghế trong 5 phút”** thành **“Đặt vé”**; giữ nguyên cơ chế giữ ghế 5 phút ở backend.
3. Vẽ lại sơ đồ ghế/giường trực quan, phù hợp từng loại xe và dữ liệu ghế thực tế đang có, tham khảo FUTA.

**Không xóa, tạo lại hoặc đổi tuyến/xe/giờ đi/giờ đến/giá/phân công của các chuyến hiện tại để phục vụ thay đổi này. Chỉ bổ sung dữ liệu điểm đón/trả và cách trình bày cần thiết.**

Sau khi hoàn thành, kiểm tra và **gửi ZIP source mới đầy đủ**, không chỉ gửi kế hoạch, code mẫu hoặc một giao diện tĩnh.

## 2. Audit source và ảnh trước khi sửa

Đọc thực tế source, tài liệu nghiệp vụ/yêu cầu gốc trong ZIP và các ảnh đính kèm. Nếu tài liệu Word/ảnh nhận xét giảng viên cũ không có trong phiên này, ghi đúng phần chưa trực tiếp đối chiếu, không giả vờ đã đọc.

Ở bản ZIP đầu vào hiện có:

- `Models/Entities.cs`: `BusRoute`, `SeatLayout`, `PhysicalSeat`, `Vehicle`, `Trip`, `TripSeat`, `SeatHold`, `Booking`, `Ticket`…
- `Trip` có `DepartureUtc`, `ArrivalUtc`, `Fare`, trạng thái/phân công; chưa có mô hình lịch điểm dừng đón/trả trong các entity này.
- `SeatLayout` có Rows/Columns/Floors; ghế vật lý có mã, hàng/cột/tầng.
- `TripSeat` lưu **snapshot hình học bất biến của ghế theo chuyến**: Code/Row/Column/Floor và liên kết tới ghế vật lý.
- Seed có sơ đồ **ghế ngồi 32 chỗ, 8 hàng × 4 ghế, 1 tầng**, và **giường nằm 24 chỗ, 4 hàng × 3 giường × 2 tầng**.
- `CatalogService.SaveLayout` sinh ghế theo hàng/cột/tầng; không cho sửa cấu trúc layout đã gắn xe. `SaveVehicle`/`SaveTrip` có các ràng buộc bảo toàn lịch và dữ liệu đã phát sinh.
- `Views/Trips/Details.cshtml`, `wwwroot/js/seats.js`, `ViewModels/Forms.cs`, `BookingService` và `SeatsApiController` xử lý chọn ghế, hold và confirm.
- Nút hiện tại là `hold-button`, nhãn “Giữ ghế trong 5 phút →”; nút xác nhận riêng là `confirm-button`.
- Có migrations SQL Server ban đầu và migration thêm `ContactMessages`; giữ phần Liên hệ và hồ sơ cập nhật database hiện có.

Kiểm tra lại vì bản được gửi có thể đã sửa. Lập danh sách file/endpoint/DTO cần thay; thay đổi tối thiểu, không xóa ràng buộc nghiệp vụ để làm UI dễ hơn.

Tham khảo https://futabus.vn/huong-dan-dat-ve-tren-web và trang đặt vé công khai nếu truy cập được, kết hợp ảnh tôi gửi. Nếu phiên đặt vé FUTA cần session thì dùng ảnh làm căn cứ visual, không giả vờ đã thao tác được. Không dùng tài khoản của tôi, không thực hiện giao dịch/giữ ghế trên hệ thống FUTA.

Chỉ học bố cục, cách chọn điểm và icon ghế; **không sao chép logo, code, dữ liệu lịch, phí, thanh toán, thương hiệu hay màu cam FUTA**. Không tự thêm trung chuyển, cổng thanh toán, FUTAPAY hoặc nút chức năng chưa có backend.

## 3. Chọn điểm đón và điểm trả — dữ liệu và giao diện thật

Ý tôi là **điểm lên xe và điểm xuống xe cụ thể thuộc chuyến đang chọn**, không phải hai dropdown đổi tuyến hoặc đổi sang chuyến khác.

Trong `Trips/Details`, thêm khối **Thông tin đón trả**:

- Desktop hai cột **Điểm đón** / **Điểm trả**; mobile xếp một cột.
- Dropdown/listbox có tên điểm, địa chỉ nếu đã có dữ liệu, giờ dự kiến tại điểm đó; có ô tìm kiếm nếu danh sách đủ dài.
- Có thể dùng native select hoặc combobox accessible phù hợp Bootstrap hiện có. Mọi phần tử phải dùng được bằng bàn phím và có label/focus rõ.
- Chỉ liệt kê điểm được phép đón/trả của **đúng chuyến, đúng chiều tuyến**. Điểm trả phải sau điểm đón theo thứ tự hành trình; không chọn cùng điểm hoặc ngược chiều.
- Sau khi chọn, cập nhật phần tóm tắt: điểm đón, giờ đón; điểm trả, giờ trả dự kiến; ngày đầy đủ; thời lượng giữa hai điểm nếu hiển thị.
- Có lời nhắc có mặt trước giờ đón theo cấu hình, ví dụ 15 phút nếu được đặt rõ là quy định của đồ án. Không nhầm giờ phải có mặt với giờ xe khởi hành tại điểm.
- Giữ riêng thông tin **giờ đi/đến của toàn chuyến** khi cần; không ghi đè khiến người dùng tưởng lịch chuyến đã đổi.
- Mặc định chọn điểm đầu/điểm cuối hợp lệ để luồng đặt vé hiện tại vẫn rõ ràng; mọi lựa chọn phải được kiểm tra lại ở server.
- Không tự hiển thị radio “Trung chuyển” hoạt động như ảnh FUTA nếu hệ thống chưa có nghiệp vụ trung chuyển. Không thêm dịch vụ đưa đón tận nhà hoặc quản lý xe trung chuyển.
- “Xem vị trí” chỉ thêm nếu có địa chỉ/tọa độ đúng và link thực sự hoạt động; không bắt buộc nhúng bản đồ hoặc thêm GPS.

Điểm đón/trả phải được lấy từ backend và lưu được cùng vé. Không chỉ gắn tên điểm vào DOM, URL hoặc localStorage rồi báo đã hoàn thành nghiệp vụ.

## 4. Giờ đón/trả phải nhất quán với chuyến, không thay lịch chuyến

Giữ nguyên `Trip.DepartureUtc` và `Trip.ArrivalUtc` của dữ liệu hiện có. Bổ sung lịch điểm dừng theo chuyến, hoặc mẫu tuyến có bản sao theo chuyến nếu phù hợp kiến trúc.

Có thể dùng mô hình tối thiểu như `TripStop` với TripId, tên/địa chỉ, thứ tự, cờ cho phép đón/trả và giờ dự kiến hoặc offset tính từ khởi hành. Đây chỉ là gợi ý, hãy chọn thiết kế ngắn gọn, không tạo một phân hệ quản lý lộ trình lớn.

Quy tắc bắt buộc:

1. Điểm đầu tương ứng giờ khởi hành toàn chuyến; điểm cuối tương ứng giờ đến dự kiến toàn chuyến.
2. Thời gian các điểm ở giữa nằm trong khoảng lịch chuyến và tăng theo thứ tự hợp lệ; nếu có thời gian dừng thì phân biệt giờ đến/giờ rời điểm rõ ràng.
3. Nếu dùng offset: `giờ tại điểm = DepartureUtc + offset đã cấu hình cho điểm`; endpoint phải khớp ArrivalUtc, không áp một offset cố định sai cho mọi tuyến/chuyến.
4. Chuyến ngược chiều có lịch/thứ tự riêng. Không lấy nguyên offset của chiều đi áp cho chiều về hoặc sắp xếp theo tên địa điểm.
5. Lưu thời gian theo UTC/UTC offset nhất quán hệ thống; hiển thị **giờ Việt Nam UTC+7**, ngày `dd/MM/yyyy`, giờ 24 giờ. Xử lý đúng qua 00:00, sang ngày/tháng/năm khác.
6. Khi khách đổi điểm, cập nhật cả **ngày và giờ**, lời nhắc có mặt trước giờ đón, thời lượng đã chọn và tóm tắt. Không chỉ thay chữ tên điểm.
7. Server tự tính/tra lịch; không tin timestamp, địa chỉ, giá hoặc thứ tự do client tự gửi lên.
8. Không sao chép giờ trên ảnh FUTA gắn cho chuyến học tập có giờ khác; không gọi thời gian suy đoán là lịch chính thức hoặc ETA theo giao thông trực tiếp.

Tôi chưa cung cấp lịch điểm dừng thực tế đầy đủ cho các chuyến trong database. Vì vậy:

- Dùng điểm/lịch đã được cấu hình đáng tin trong source/database nếu có.
- Với tuyến học tập được nhận diện rõ, được bổ sung **lịch mô phỏng có kiểm soát**, khớp chính xác giờ đầu/cuối và tổng thời gian chuyến, ghi rõ là dữ liệu học tập; không tự nhận đây là lịch nhà xe thật. Không tự chia đều thời gian theo số điểm rồi gọi là chính xác thực tế.
- Ghi nguồn/căn cứ cho tên và địa chỉ công khai nếu dùng địa điểm có thật; không khẳng định nhà xe đồ án có văn phòng/điểm đón tại đó.
- Với tuyến người dùng tạo chưa có lịch điểm: dùng điểm đầu/cuối trung tính theo tên tuyến và giờ đã có, thể hiện chưa cấu hình điểm trung gian. Không bịa danh sách địa phương cho tuyến đó.
- Bàn giao bảng đối chiếu TripId, tuyến, giờ đầu/cuối giữ nguyên, các điểm/thời gian bổ sung và trạng thái lịch mô phỏng/cấu hình để tôi kiểm tra.

Giữ quy tắc mở bán/đóng bán hiện có. Việc chọn điểm giữa **không tự cho phép đặt sau giờ khởi hành toàn chuyến** hoặc thay đổi chính sách đổi/hủy, vì tôi không yêu cầu mở rộng phần đó.

## 5. Lưu lựa chọn vào vé và bảo toàn dữ liệu cũ

Chọn phạm vi tối thiểu: các ghế trong cùng một đơn đặt vé dùng chung một cặp đón/trả; nếu cần khác nhau, người dùng đặt đơn khác. Không tự mở rộng chọn điểm riêng cho từng hành khách.

- Bổ sung DTO/ViewModel và luồng xác nhận để gửi ID điểm đã chọn; server kiểm đúng TripId của hold, điểm đang hoạt động, cờ đón/trả, thứ tự và lịch.
- Khi xác nhận mới, lưu lựa chọn và **snapshot tên/địa chỉ/giờ đón/trả** phù hợp để vé cũ không thay đổi theo chỉnh sửa danh mục về sau.
- Hiển thị lại ở chi tiết vé, Vé của tôi khi phù hợp, phòng vé và danh sách hành khách của Admin/Staff/Driver theo quyền hiện có.
- Khôi phục lựa chọn hợp lệ sau validation/network error/refresh trong phạm vi có thể, không lấy localStorage làm nguồn xác nhận. Khôi phục an toàn khi người dùng có nhiều lượt giữ cùng chuyến.
- Đổi điểm trong bước nhập thông tin không tự tạo hold mới, không nhả ghế, gia hạn TTL hoặc confirm vé. Hold hết hạn vẫn phải chọn/đặt lại đúng service.
- Khi đổi vé sang chuyến mới, chọn lại điểm thuộc chuyến mới; không mang ID điểm của chuyến cũ sang. Đổi thất bại giữ nguyên vé cũ.
- Sau confirm, retry cùng yêu cầu phải trả lại booking đã tạo theo idempotency hiện có; không tạo vé trùng hoặc sửa snapshot booking cũ theo payload retry khác. Nếu payload thay đổi thì xử lý rõ ràng, không ghi đè âm thầm.

**Không thêm bán vé theo chặng/segment inventory hay định giá theo quãng đường.** Quyền chiếm ghế tiếp tục theo toàn chuyến, giá/ghế và tổng tiền như cũ. Hai khách đi hai đoạn không giao nhau vẫn không được bán trùng ghế trên cùng chuyến theo mô hình hiện tại. Giao diện/chính sách cần diễn đạt rõ giá đang áp dụng cho vé của chuyến, không tự hiển thị giá chặng giả.

Vé đã có trước nâng cấp tiếp tục đọc được, không mất mã/khách/ghế/giá/trạng thái/thu tiền. Cho phép cột bổ sung nullable với dữ liệu cũ và hiển thị lịch toàn chuyến hoặc “Chưa ghi nhận điểm đón/trả” phù hợp; không backfill rồi khẳng định khách cũ từng chọn điểm cụ thể. Yêu cầu điểm hợp lệ chỉ áp dụng đúng luồng confirm mới, không làm hỏng retry một booking đã xác nhận.

## 6. Đổi nhãn nút thành “Đặt vé”, giữ nguyên hold 5 phút

Tại trang chọn ghế, đổi nhãn nút:

**“Giữ ghế trong 5 phút →” → “Đặt vé”**

Đây là đổi cách gọi trên giao diện, **không xóa cơ chế giữ chỗ** — vì giữ chỗ theo thời gian thực là trọng tâm đề tài.

Luồng mong muốn:

1. Khách chọn ghế, chọn điểm đón/trả hợp lệ.
2. Nhấn **Đặt vé** → server gọi service giữ ghế hiện có, giữ tối đa 6 ghế toàn bộ hoặc không giữ, TTL 300 giây.
3. Server thành công → hiển thị rõ vùng nhập liên hệ/hành khách, thời gian còn lại và nút **Xác nhận đặt vé**; có thể cuộn/focus hợp lý vào phần tiếp theo.
4. Chỉ khi confirm thành công mới có booking/mã vé. Nhấn Đặt vé lần đầu không đồng nghĩa vé đã xác nhận hoặc đã thanh toán.

- Giữ nút confirm riêng, không gộp Acquire/Confirm theo cách bỏ validation/hành khách.
- Có thể đổi “Đăng nhập để giữ ghế” thành “Đăng nhập để đặt vé” cho nhất quán, bảo toàn returnUrl và phân quyền.
- Nút có loading/disabled khi đang gửi, ngăn submit trùng, lỗi trả về hiển thị thật.
- Khi có active hold, vùng thao tác phải cho thấy đang tiếp tục lượt giữ, không giữ một nút vô hiệu khó hiểu và ẩn form phía dưới.
- **Không xóa thông tin giữ chỗ 5 phút, countdown, hạn server hoặc nội dung giải thích cần thiết.** Chỉ thay nhãn CTA được yêu cầu, không tìm/replace tất cả chữ “giữ ghế” trong nghiệp vụ/tài liệu.

Ảnh tôi gửi có trạng thái “Server đã giữ ghế” nhưng tóm tắt vẫn “Chưa chọn”, tổng 0 và nút bị vô hiệu. Hãy tái hiện, audit `selected`/`activeHold`/snapshot và sửa phần đồng bộ UI liên quan:

- Tóm tắt khi có active hold phải lấy đúng ghế/giá từ lượt giữ đang chọn, kể cả khi `selected` đã được clear sau Acquire.
- Khôi phục hold sau refresh phải hiển thị ghế của tôi, tổng tiền, countdown, điểm đón/trả và form tương ứng.
- Chuyển lượt giữ cập nhật đúng tóm tắt; không ghi đè hành khách đã nhập khi chỉ nhận bản tin realtime không liên quan.
- Chỉ báo đang giữ khi server xác nhận; không tạo kết quả thành công giả hoặc coi ảnh chụp là bằng chứng đã tìm được nguyên nhân chính xác.

## 7. Sơ đồ ghế/giường trực quan, đúng loại xe

Tham khảo hình FUTA tôi gửi về icon ghế/giường, tầng dưới/tầng trên, lối đi, khoảng trống và trạng thái. Thiết kế riêng theo gam trắng–xanh đang có; không copy ảnh sơ đồ FUTA làm background với nút chồng lên.

### Xe ghế ngồi một tầng

- Có đầu xe, vị trí tài xế/vô lăng mang tính minh họa, hướng xe và lối đi rõ.
- Với layout 32 chỗ hiện tại 8 × 4: thể hiện **2 ghế — lối đi — 2 ghế** từng hàng, đúng 32 nút, mã ghế/ID/tọa độ nghiệp vụ giữ nguyên.
- Icon ghế có tựa/lưng và đệm, nhìn rõ hướng; code dễ đọc. Vị trí tài xế không phải ghế bán.
- Không tự thêm tầng trên, đổi thành giường hoặc tăng/giảm số ghế.

### Xe giường nằm hai tầng

- Với layout 24 chỗ hiện tại: hiển thị đúng 12 giường mỗi tầng, 24 tổng cộng, theo Row/Column/Floor và cấu hình đang có.
- Desktop hai sơ đồ cạnh nhau ghi **Tầng dưới / Tầng trên**; mobile có thể xếp dọc hoặc tab accessible, chọn ghế tầng kia không bị mất.
- Icon giường phù hợp, các dãy/lối đi rõ, hướng đầu xe nhất quán. Không tự mô tả một layout học tập là đúng mẫu xe thương mại cụ thể khi chưa có thông số.
- Mã A/B hiển thị theo dữ liệu thật. Không áp cố định sơ đồ 34 chỗ A01–A17/B01–B17 trong ảnh tham khảo cho xe chỉ có 24 giường.

### Renderer và dữ liệu ghế

- Render từ `TripSeat` snapshot của chuyến và metadata layout có liên quan, không hard-code DOM theo một TripId/biển số/tên xe.
- Tạo khoảng trống/lối đi bằng CSS Grid hoặc metadata trình bày; **ô lối đi không được tạo thành `PhysicalSeat`/`TripSeat`, không tăng capacity**.
- Không sắp xếp lại hàng/cột/mã để “trông giống FUTA” rồi làm vé cũ đổi vị trí. Mỗi phần tử bấm phải ánh xạ một-một đúng TripSeatId.
- Giữ `PhysicalSeatId`, TripSeatId, Code, Row, Column, Floor, trạng thái, hold và ticket reference hiện có.
- Rà giới hạn Columns/Floors/check constraints trước khi thêm metadata; không sửa Columns trong DB thành số cột gồm cả lối đi nếu cột đó hiện đếm ghế.
- Có thể bổ sung kiểu hiển thị seat/berth hoặc gap vào layout nếu thực sự cần; chỉ thay metadata trình bày an toàn. Không sửa đại trà hình học bất biến của chuyến đã có hold/vé.
- Nếu một layout không đủ thông tin để biểu diễn đúng xe thực tế, dùng cách hiển thị trung thực từ dữ liệu, ghi rõ giới hạn; thay cấu trúc thật chỉ cho layout mới/chuyến tương lai theo quy tắc hiện có, không tạo lại chuyến cũ.
- Preview sơ đồ trong Admin nếu có phải khớp renderer trang khách; giữ các quy tắc không sửa layout đã gắn xe/xe đã có chuyến để bảo toàn lịch sử.

### Trạng thái và thao tác

- Phân biệt: Trống, Đang chọn, Đang giữ bởi người khác, Lượt giữ của tôi, Đã đặt.
- Dùng màu + chữ/icon/chú giải; không chỉ dựa vào màu hoặc hover. Bàn phím, focus và aria-label đủ mã/tầng/trạng thái.
- Ghế không còn trống không bấm được; thao tác từ tab khác/realtime cập nhật đúng, không chỉ tô màu giả.
- Tổng ghế, số trống, mã ghế và total luôn khớp database/snapshot.
- Hover/animation nhẹ; hỗ trợ Reduce Motion, không rung/lật/scale mạnh, không che nội dung ghế.

## 8. Cập nhật database hiện có an toàn, có kiểm soát

Việc thêm lịch điểm dừng và lưu điểm đón/trả có thể cần schema. Được bổ sung tối thiểu, nhưng phải sửa đồng bộ Entity, DbContext, DTO/ViewModel, service/Controller, API, Razor/JS, EF migration/snapshot và test.

- Chỉ dùng **migration tăng thêm** cho bảng/cột cần thiết; không tạo Initial migration mới, không xóa/sửa history hoặc baseline đang dùng.
- Giữ migrations hiện có, bao gồm `AddContactMessages`; không làm mất chức năng Liên hệ hoặc dữ liệu đã gửi.
- Migration không được drop/recreate Trips, TripSeats, PhysicalSeats, Bookings, Tickets, Identity hoặc Payments; không đổi giá/phân công/lịch chuyến cũ.
- Rà SQL sinh ra, hướng dẫn backup và áp dụng đúng migration mới bằng EF cho database đang có. Không chạy SQL legacy chồng EF; không đặt `EnsureCreated` cùng `Migrate` trên cùng database.
- Không tự áp schema vào database riêng của tôi hoặc thay connection string khi chưa có phạm vi/môi trường đó. Dùng database test riêng khi kiểm thử ghi.
- Update/backfill điểm dừng của tuyến học tập phải nhận diện rõ, **idempotent**, không chỉ sửa SeedData rồi bỏ mặc database đã có.
- Nếu khởi tạo dữ liệu bổ sung, dùng command/quy trình rõ ràng với transaction ngắn và khóa duy nhất như TripId + StopOrder; chạy lại không trùng và không ghi đè điểm người dùng đã sửa.
- Hỗ trợ chuyến đang tồn tại và chuyến mới tạo qua Admin; không chỉ bổ sung cho các TripId seed cố định.
- Phải có cách cấu hình lịch điểm trong form chuyến/quản lý sẵn có ở mức tối thiểu, hoặc quy trình nhập có validation rõ. Không buộc người dùng sửa code mỗi khi thêm chuyến.
- Điểm đã được booking tham chiếu không xóa cascade; giữ snapshot lịch sử. Sửa lịch khi có hold/booking phải có quy tắc bảo vệ/phiên bản và kiểm tra lại lúc confirm, không âm thầm làm sai thông tin khách đã chọn.

## 9. Bảo toàn kỹ thuật giữ chỗ và các chức năng còn lại

Các yêu cầu gốc tiếp tục có hiệu lực:

- SQL Server là nguồn quyết định trạng thái/quyền giữ/đặt ghế.
- Giữ chỗ **300 giây**, hạn lưu bền vững, thời gian server chuẩn; không giữ khóa/transaction/kết nối mở 5 phút.
- Giữ transaction ngắn, constraint/locking, ownership, token và idempotency. Double booking phải bằng 0.
- Worker expiry và kiểm hết hạn trong transaction vẫn đúng; Booked không bị expiry của hold cũ trả về trống.
- SignalR chỉ đồng bộ sau commit, group đúng chuyến, reconnect lấy snapshot; không public hành khách/hold token/điểm đã chọn của tài khoản khác.
- Các kênh khách hàng/phòng vé dùng chung service; không có lối tắt ghi trực tiếp trạng thái ghế.
- Giữ quyền Admin/Staff/Driver/Customer, ownership vé, anti-forgery, redirect login, đổi/hủy nguyên tử, thu/hoàn trực tiếp và dashboard.
- Không thêm thanh toán ngân hàng, GPS, bản đồ theo dõi realtime, trung chuyển, CRM, AI, marketplace hoặc ứng dụng di động.
- Không thay đổi ảnh/quyền tài sản, thương hiệu, Liên hệ hoặc UI các trang không liên quan chỉ vì đang sửa trang chọn ghế.

## 10. Kiểm thử và bằng chứng bắt buộc

Thử restore/build/run theo cấu hình project .NET 9/SQL Server hiện có. Kiểm migration model/snapshot; không dùng SQLite/InMemory/mock backend để thay bằng chứng SQL Server thật.

| Nhóm | Kịch bản cần kiểm |
|---|---|
| Bảo toàn chuyến | So sánh trước/sau: ID, tuyến, xe, giờ đi/đến, giá, trạng thái, phân công không bị đổi bởi update |
| Điểm đón/trả | Đúng chuyến/chiều, không cùng điểm/ngược thứ tự, từ chối ID chuyến khác, điểm ngừng dùng/giờ giả client |
| Thời gian | Điểm đầu/cuối khớp giờ chuyến; điểm giữa hợp lệ; đổi điểm cập nhật giờ và ngày; qua nửa đêm và chiều ngược |
| Dữ liệu bổ sung | Database cũ cập nhật được; chạy update lần 2 không trùng; không ghi đè điểm sửa tay; chuyến mới có quy trình cấu hình |
| Nút Đặt vé | Chỉ thay nhãn; nhấn vẫn Acquire 300 giây, form/countdown hiện đúng; confirm mới tạo booking |
| Tóm tắt hold | Sau Acquire/refresh/chuyển hold, ghế và total không thành “Chưa chọn / 0” sai; input không mất khi sync |
| Lưu vé | Điểm và giờ được lưu snapshot, đọc lại đúng trong chi tiết vé/phòng vé/danh sách khách; vé cũ vẫn đọc được |
| Đổi vé | Chọn điểm chuyến mới, reject ID cũ, lỗi không mất vé cũ, tiền và số ghế giữ quy tắc cũ |
| Ghế ngồi | Layout 32 chỗ một tầng: đúng 32 nút, mã/ID/capacity giữ nguyên, 2+2 và lối đi rõ |
| Giường nằm | Layout 24 chỗ hai tầng: 12/tầng, đúng mã/tầng/hàng, đổi tab không mất selection |
| Quyền/realtime | Hai tài khoản tranh cùng ghế, hold hết hạn/release, reconnect, không rò dữ liệu, retry không tạo vé trùng |
| Hồi quy | Login/role, Customer đặt vé, Staff bán/thu/hoàn, Driver chuyến được giao, Admin và Liên hệ vẫn hoạt động |
| Browser/UI | 375px, 390px, 768px, 1024px, 1440px; bàn phím/focus/Reduce Motion, không console error/ảnh lỗi/tràn ngang |
| HTTP | Không phát sinh 500 trên trang hợp lệ; validation/quyền/conflict/database lỗi trả đúng, không báo success giả |

Khi có môi trường SQL Server, chạy lại test tranh chấp 20/50/100 request đồng thời theo bộ test cũ, ghi kết quả thực và DB double booking = 0. Không tự đổi số đo cũ thành kết quả mới hoặc coi kiểm tra cú pháp là concurrency test.

Chụp screenshot website chạy thật: chọn ghế xe ngồi, xe giường hai tầng, dropdown đón/trả, tóm tắt sau chọn, sau nhấn Đặt vé và chi tiết vé đã lưu điểm. Dùng dữ liệu học tập, không ảnh mockup hoặc dữ liệu cá nhân thật.

**Chỉ PASS khi đã chạy kiểm chứng.** Nếu thiếu SQL Server/runtime/browser, ghi rõ **CHƯA KIỂM CHỨNG** cho phần tương ứng, nêu lỗi chặn và bước tôi cần chạy; không tuyên bố đã hết lỗi 500.

## 11. Tài liệu cập nhật theo source

- `README.md`: mở solution .NET 9, giữ SQL Server, cập nhật migration/dữ liệu mới và thao tác đón/trả/Đặt vé.
- `PICKUP_DROPOFF_GUIDE.md`: mô hình điểm, quy tắc thứ tự/giờ, lịch mô phỏng, cách cấu hình, bảng đối chiếu các chuyến hiện có, giới hạn không bán ghế theo chặng.
- `SEAT_LAYOUT_REVIEW.md`: mapping loại xe/layout/renderer, capacity/mã ghế trước-sau, quy tắc không đổi snapshot và khác biệt so với mẫu FUTA.
- `DATABASE_CHANGELOG.md`: migration mới, cột nullable cho vé cũ, update idempotent, backup, cách áp dụng không mất dữ liệu.
- `UI_CHANGELOG.md`, `FILE_CHANGES.md`: file CSS/JS/Razor/API/service thực sự đổi và lý do; xác nhận phạm vi đổi nhãn nút.
- `DESIGN_REFERENCE_REVIEW.md`: URL FUTA/ảnh đã khảo sát, bố cục tham khảo và phần không sao chép.
- `TEST_RESULTS.md`: expected/actual, PASS/FAIL/CHƯA KIỂM CHỨNG, môi trường, log và screenshot thật.
- Cập nhật sơ đồ dữ liệu/traceability/permission/hold/realtime khi thay đổi thực sự liên quan, giữ tài liệu yêu cầu gốc và báo cáo concurrency trung thực.
- Giữ `ASSET_SOURCES.md`, giấy phép ảnh/icon; nếu tự vẽ icon ghế mới thì ghi đúng nguồn, không dùng ảnh/sprite FUTA chưa có quyền.

## 12. Bàn giao ZIP mới

Xuất một ZIP source hoàn chỉnh:

**`QuanLyXeKhach_DatVe_GiuCho_REALTIME_STOPS_SEATS_REVIEWED.zip`**

Chỉ dùng hậu tố **`VERIFIED`** khi đã thật sự restore/build/run với SQL Server và kiểm thử đạt luồng chọn điểm, Acquire/Confirm, lưu vé, quyền, các loại sơ đồ và hồi quy cốt lõi. Ghi riêng mục chưa thử.

ZIP phải chứa solution/project hiện tại, toàn bộ MVC/Razor/Identity/EF SQL Server, migrations/snapshot/update dữ liệu, worker/Hub/Services/Controllers, CSS/JS/ảnh local/thư viện/animation, Liên hệ đang có, test và tài liệu.

Không đóng gói bin/obj/.vs/node_modules, secrets, database/backup riêng hoặc dữ liệu cá nhân thật. Không hard-code connection string/mật khẩu của tôi. Giữ cách mở solution và profile chạy hiện tại, không yêu cầu dựng một project mới.

Phản hồi cuối gồm:

1. Link tải ZIP source đã sửa.
2. Tóm tắt điểm đón/trả, quy tắc thời gian và các chuyến được bổ sung dữ liệu; xác nhận không xóa/đổi lịch chuyến.
3. Nêu rõ **nút Đặt vé vẫn dùng cơ chế giữ chỗ 5 phút**, chỉ confirm mới tạo vé.
4. Sơ đồ ghế trước/sau theo loại xe, capacity và cách bảo toàn mã/ID/vé cũ.
5. Kết quả build/run/SQL Server/HTTP/browser, bảng PASS/FAIL/CHƯA KIỂM CHỨNG và screenshot thật nếu có.
6. Hướng dẫn backup, cập nhật database hiện có, mở Visual Studio và chạy lại.
7. Giới hạn còn lại, đặc biệt lịch mô phỏng và phần chưa có thông số xe/lịch vận hành thật.

**Ưu tiên giữ website và dữ liệu ổn định. Chỉ bổ sung điểm đón/trả, đổi nhãn CTA và sửa sơ đồ ghế theo yêu cầu này. Giữ nguyên tên đề tài, chuyên ngành, công nghệ và toàn bộ nghiệp vụ/yêu cầu còn lại. Hãy thực hiện trên source đính kèm và bàn giao ZIP mới; không chỉ cung cấp một prompt tiếp theo hoặc mockup.**
