**# YÊU CẦU TRIỂN KHAI WEBSITE ĐỒ ÁN CHUYÊN NGÀNH THƯƠNG MẠI ĐIỆN TỬ**



**## 1. Giữ nguyên tên đề tài và căn cứ yêu cầu**



Tôi cần bạn trực tiếp xây dựng một website ASP.NET Core MVC hoàn chỉnh, có backend và SQL Server, để thực hiện đồ án chuyên ngành Thương mại điện tử.



**\*\*Tên đề tài chính thức phải giữ nguyên từng ý, không rút gọn hoặc đổi thành đề tài khác:\*\***



**\*\*XÂY DỰNG HỆ THỐNG QUẢN LÝ XE KHÁCH VÀ ĐẶT VÉ TRỰC TUYẾN TÍCH HỢP CƠ CHẾ GIỮ CHỖ THEO THỜI GIAN THỰC\*\***



Căn cứ bắt buộc:



1. File \`1250070011_TranTrungHieu_12TMDT_DeCuongDoAnChuyenNganh.docx\` tôi đính kèm.

2. Ảnh nhận xét của giảng viên tôi đính kèm, phần **\*\*14. TRẦN TRUNG HIẾU\*\***.

3. ZIP \`EsportsPortal_V2_PREVIEW_READY_REVIEWED(2).zip\` chỉ để tham khảo cách tổ chức source Visual Studio, MVC/Razor, giao diện, tài liệu, đóng gói và cách báo cáo kiểm thử trung thực.



**\*\*Word và ảnh nhận xét quyết định nghiệp vụ. Không dùng nghiệp vụ Esports trong ZIP để thay đổi đề tài xe khách.\*\***



Hãy đọc thực tế các tài liệu trước khi thiết kế. Nếu không mở được tài liệu, nói rõ tài liệu nào chưa đọc; không tuyên bố đã kiểm tra. Ảnh nhận xét có phần bị cắt và đánh số không liên tục; không tự bịa mục bị thiếu. Đối chiếu phần đánh giá định lượng với nội dung đầy đủ trong Word.



Không tự sửa đề cương, đổi tên đề tài, bỏ yêu cầu giảng viên hoặc bổ sung một đề tài mới. Các quyết định kỹ thuật cần thiết để thực hiện yêu cầu phải được giải thích và truy vết; nếu có mâu thuẫn thực sự làm thay đổi phạm vi hoặc công nghệ đã đăng ký, nêu rõ để tôi quyết định trước phần phụ thuộc.



**## 2. Tôi muốn nhận gì ở giai đoạn này**



Tôi cần **\*\*source website thực sự\*\***, tương tự hình thức bàn giao ZIP của project trước: giải nén, mở solution bằng Visual Studio, cấu hình SQL Server, restore, build và chạy được các chức năng.



Tôi chưa yêu cầu làm video trình diễn, slide bảo vệ, hosting, tunnel hoặc một chế độ Demo/Preview riêng. **\*\*Không đưa Demo SQLite, EF InMemory, database JSON, chế độ chỉ đọc hoặc cơ chế tự fallback từ project Esports vào hệ thống này.\*\***



Điều này không có nghĩa là bỏ dữ liệu mẫu và kiểm thử: dữ liệu kiểm thử trên SQL Server, tình huống trình diễn nghiệp vụ và testcase/concurrency test vẫn phải có theo đề cương và nhận xét. Đây là dữ liệu phục vụ kiểm chứng của website, không phải một backend giả thay cho SQL Server.



Không chỉ gửi prompt tiếp theo, hướng dẫn chung, mockup, HTML tĩnh hoặc các đoạn code rời. Hãy triển khai source trong phiên làm việc, xử lý lỗi tìm được và bàn giao một ZIP hoàn chỉnh. Không tự chỉnh sửa file Word gốc nếu tôi chưa yêu cầu.



**## 3. Phạm vi và giới hạn không được tự thay đổi**



Phạm vi đúng đề cương gồm:



- Quản lý tuyến, xe, sơ đồ ghế và chuyến xe.

- Tra cứu chuyến theo nơi đi, nơi đến và ngày đi.

- Xem sơ đồ ghế của từng chuyến.

- Giữ chỗ tạm thời **\*\*5 phút\*\***.

- Đặt vé trực tuyến trong phạm vi mô phỏng nghiệp vụ, ghi nhận thanh toán trực tiếp bởi nhân viên.

- Quản lý vé/đặt vé, hỗ trợ bán vé và đổi/trả theo quy định đã công bố trong hệ thống.

- Phân công tài xế/nhân viên và danh sách hành khách.

- Tác nghiệp tài xế trên chuyến được phân công.

- Bốn vai trò: Admin, Nhân viên, Tài xế, Khách hàng.

- Dashboard thống kê vé và doanh thu.

- Chống tranh chấp ghế ở database; SignalR đồng bộ giao diện sau commit.

- Kiểm thử chức năng, tải, tranh chấp và đánh giá định lượng.



**\*\*Không thêm:\*\*** cổng thanh toán ngân hàng thật, POS tại bến, GPS/bản đồ theo dõi xe trực tiếp, quản lý nhiên liệu, bảo dưỡng/sửa chữa, AI/chatbot, marketplace nhiều nhà xe, ví điện tử, loyalty/voucher, SMS trả phí, forum/tin Esports/Favorite hoặc ứng dụng di động riêng.



Không biến “đặt vé trực tuyến” thành giao diện chọn ghế không lưu dữ liệu. Không gọi một màn hình đếm ngược JavaScript là đã triển khai giữ chỗ thời gian thực.



**## 4. Công nghệ triển khai .NET 9**



Sử dụng:



- **\*\*ASP.NET Core MVC .NET 9, C#, TargetFramework \`net9.0\`.\*\***

- Razor Views, HTML5, CSS3, JavaScript.

- Entity Framework Core 9 và SQL Server Provider 9, đồng bộ các package Microsoft liên quan ở phiên bản 9.0.x tương thích.

- ASP.NET Core Identity 9, Cookie Authentication và RBAC.

- ASP.NET Core SignalR trên .NET 9; JavaScript client 9.x tương thích và đóng gói local.

- Microsoft SQL Server.

- Bootstrap 5; Chart.js cho biểu đồ thống kê cần thiết.

- \`async/await\` cho truy cập dữ liệu và các tác vụ I/O.

- Visual Studio 2022 17.14 phù hợp SDK .NET 9.0.305, SSMS; kiểm thử HTTP và tải bằng công cụ phù hợp như Postman/k6/JMeter/Locust.



**\*\*Tôi đã xác nhận chuyển nền tảng triển khai từ .NET 8 trong đề cương sang .NET 9. Đây chỉ là thay đổi phiên bản công nghệ; giữ nguyên tên đề tài, nghiệp vụ, yêu cầu giảng viên và toàn bộ phạm vi còn lại. Không tự chuyển sang .NET 10 hoặc sửa file Word gốc.\*\***



Kiểm SDK/runtime và Visual Studio trước khi cấu hình \`global.json\`. Ưu tiên SDK .NET 9.0.305 đã có trên máy tôi, dùng rollForward phù hợp trong .NET 9. Không ghim một SDK không có trên máy mà không hướng dẫn cài đặt. Nếu thiếu thành phần cần thiết, hướng dẫn bổ sung .NET 9; không âm thầm đổi TargetFramework khỏi \`net9.0\`. Không sửa thư mục SDK hoặc \`Microsoft.NET.Sdk.DefaultItems.targets\`.



Không bắt buộc React/Vue, microservices, Redis, Kafka hoặc hạ tầng phân tán. Ưu tiên một ứng dụng MVC có cấu trúc rõ, đủ chức năng và chứng minh được tính đúng đắn.



Tài liệu kỹ thuật chính thức để đối chiếu, không thay thế đề cương:



- SQL Server table hints: [https\://learn.microsoft.com/en-us/sql/t-sql/queries/hints-transact-sql-table]\(https\://learn.microsoft.com/en-us/sql/t-sql/queries/hints-transact-sql-table)

- SignalR JavaScript client: [https\://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client?view=aspnetcore-9.0]\(https\://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client?view=aspnetcore-9.0)

- Background services: [https\://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-9.0]\(https\://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-9.0)



**## 5. Kiến trúc source và Visual Studio**



Có solution và project mở được bằng Visual Studio. Có thể dùng tên kỹ thuật \`BusBooking.sln\` / \`BusBooking.csproj\`, nhưng tên đề tài trên tài liệu và giao diện giới thiệu vẫn giữ nguyên.



Source tối thiểu gồm:



- \`Program.cs\`, \`appsettings.json\`, \`appsettings.Development.json\`, \`global.json\`, \`Properties/launchSettings.json\`.

- Models/Entities, DbContext, SQL Server migrations và cấu hình Identity.

- Controllers, ViewModels/DTOs, Razor Views.

- Services xử lý đặt vé, giữ chỗ, hủy/đổi vé và thống kê.

- BackgroundService xử lý hold hết hạn.

- SignalR Hub và JavaScript đồng bộ sơ đồ ghế.

- Khu vực hoặc nhóm giao diện theo vai trò Admin/Nhân viên/Tài xế/Khách hàng.

- \`wwwroot\` chứa CSS, JS, thư viện và hình ảnh local.

- Seed dữ liệu kiểm thử có kiểm soát, bộ test và tài liệu.



Không để Controller hoặc Hub tự chứa toàn bộ nghiệp vụ. Các kênh web, phòng vé và nhân viên phải dùng chung service/transaction để không có đường bán vé bỏ qua chống trùng.



Chỉ tận dụng thành phần chung từ ZIP tham khảo khi thực sự phù hợp và có quyền sử dụng. Không đổi tên Games thành Vehicles rồi giữ lại quan hệ/migration Esports. Không giữ tên EsportsPortal, logo Riot, ảnh game, 15 bảng Esports hoặc tài khoản Esports trong project xe khách.



**## 6. Mô hình dữ liệu bắt buộc đúng bản chất**



Phân biệt rõ các thực thể:



1. **\*\*Tuyến\*\***: điểm đi, điểm đến và thông tin tuyến.

2. **\*\*Xe\*\***: phương tiện, biển số giả lập khi seed, thông tin loại xe và sơ đồ ghế.

3. **\*\*Sơ đồ ghế\*\***: cấu trúc bố trí, hàng/cột/tầng nếu áp dụng.

4. **\*\*Ghế vật lý\*\***: vị trí ghế thuộc sơ đồ tương ứng.

5. **\*\*Chuyến xe\*\***: một lần khai thác tuyến, xe, thời gian khởi hành, giá và phân công.

6. **\*\*Ghế chuyến\*\***: một ghế trên một chuyến cụ thể, có trạng thái và quyền giữ/đặt riêng.

7. **\*\*Giữ chỗ\*\***: người giữ, định danh hold, thời điểm bắt đầu/hết hạn và trạng thái vòng đời cần thiết.

8. **\*\*Đặt vé/vé\*\***: khách hàng, hành khách, chuyến/ghế, giá chốt, mã tra cứu, trạng thái và liên hệ thanh toán.

9. Dữ liệu phân công, thanh toán trực tiếp và lịch sử thay đổi cần thiết cho các chức năng đã đăng ký.

10. ASP.NET Identity cho tài khoản và vai trò.



Được chọn cách tách bảng hợp lý nhưng phải có ERD, khóa chính/ngoại, unique/index/check và giải thích. Không bắt buộc đúng một số lượng bảng kế thừa từ Esports.



Các bất biến dữ liệu:



- Cặp chuyến–ghế chỉ có một dòng Ghế chuyến tương ứng.

- Cùng vị trí ghế trên hai chuyến khác nhau có trạng thái độc lập.

- Một ghế chuyến không có hai người giữ hợp lệ hoặc hai vé còn hiệu lực đồng thời.

- Vé phải tham chiếu đúng ghế thuộc đúng chuyến, không cho gửi TripId và SeatId không khớp.

- Tồn tại ràng buộc duy nhất ở database cho vé còn hiệu lực trên ghế chuyến, hoặc thiết kế tương đương có thể chứng minh và kiểm thử.

- Hủy vé giữ lịch sử, không xóa hàng loạt bản ghi giao dịch.

- Giá tiền dùng kiểu decimal phù hợp; tổng tiền tính phía server, không tin giá gửi từ trình duyệt.

- Thời điểm nghiệp vụ lưu UTC, hiển thị giờ Việt Nam rõ ràng.

- Có index hỗ trợ tìm chuyến, truy vấn ghế chuyến, hold hết hạn và báo cáo.

- Không để sửa sơ đồ/đổi xe làm sai vé đã bán. Bảo toàn cấu hình ghế của chuyến đã phát sinh nghiệp vụ; khóa các sửa đổi không an toàn hoặc có quy trình rõ trong phạm vi đề tài.

- Phân công và lịch trình phải kiểm hợp lệ; không cho xe/tài xế bị xếp lịch chồng chéo rõ ràng. Nếu cần khoảng đệm/thời gian dự kiến, công bố cách cấu hình, không tự bịa quy định nhà xe.



**## 7. Trạng thái và quy trình đặt vé**



**### Trạng thái ghế chuyến**



Giữ rõ ba trạng thái theo đề cương:



- \`Available\`: ghế trống.

- \`Held\`: đang được giữ tạm và chưa hết hạn.

- \`Booked\`: đã đặt/xác nhận vé.



Luồng:



- Available → Held: giữ thành công.

- Held → Available: hết hạn hoặc chủ sở hữu trả chỗ hợp lệ.

- Held → Booked: chủ sở hữu hoàn tất đặt vé trước thời hạn.

- Booked → Available: hủy/đổi vé hợp lệ và giải phóng ghế trong cùng giao dịch liên quan.



Phân biệt trạng thái hold, ghế, vé/đơn đặt vé và thanh toán. Không dùng một cột duy nhất để thay cho mọi vòng đời.



Khách đăng nhập, chọn chuyến, chọn ghế, giữ chỗ, điền thông tin hành khách rồi xác nhận đặt vé. Có trang xác nhận/mã vé và lịch sử đặt vé. Nếu giữ nhiều ghế trong một lần, yêu cầu rõ chính sách và thực hiện **\*\*all-or-nothing\*\***, không âm thầm giữ được một phần rồi thông báo giữ toàn bộ thành công.



Đặt vé thành công có thể ghi nhận chưa thanh toán và chờ thanh toán trực tiếp; nhân viên xác nhận đã thu tiền. **\*\*Ghế đã Booked không tự bị trả về Available chỉ vì mốc giữ chỗ 5 phút cũ hết hạn.\*\*** Không tự thêm thời hạn thanh toán/hủy tự động ngoài yêu cầu mà không giải thích và được chấp nhận.



Chính sách hủy/đổi và xử lý tiền phải được công bố nhất quán. Không tự gán mức phạt, mức hoàn hay quy định pháp lý không có trong tài liệu. Trong phạm vi học tập, xử lý xác nhận thu/hoàn/đối soát thủ công bởi người có quyền, không mô tả như chuyển tiền ngân hàng thật.



**## 8. Yêu cầu kỹ thuật giữ chỗ 5 phút — TRỌNG TÂM**



**\*\*Giữ chỗ 5 phút không đồng nghĩa giữ SQL transaction hoặc row lock trong 5 phút.\*\***



Bắt buộc:



1. Lưu định danh hold, chủ sở hữu và \`ExpiresAt\` bền vững trong SQL Server.

2. Thời hạn mặc định thực tế là **\*\*300 giây\*\***; reload, mở tab khác hoặc reconnect không được tự cộng lại 5 phút.

3. Dùng thời gian UTC có thẩm quyền ở server/database. Không lấy thời gian máy khách để quyết định hết hạn.

4. Theo đề cương, sử dụng **\*\*transaction ngắn với \`UPDLOCK\`, \`ROWLOCK\`\*\*** phù hợp để kiểm tra/cập nhật ghế chuyến. Việc kiểm tra và thay đổi trạng thái phải nguyên tử.

5. Ghế chuyến phải có trước khi mở bán để khóa/truy vấn đúng dòng đã tồn tại; có unique/index phù hợp.

6. Chỉ giữ lock trong đoạn kiểm tra–cập nhật–commit/rollback. Không chờ người dùng nhập form, đếm ngược, gọi dịch vụ ngoài hoặc gửi SignalR trong transaction đó.

7. Không dùng \`Task.Delay(5 phút)\`, \`Thread.Sleep\`, kết nối SQL mở xuyên nhiều request, khóa C# theo process hoặc SignalR connection ID làm cơ chế sở hữu ghế.

8. Không coi \`ROWLOCK\` là bảo đảm SQL Server không bao giờ khóa key/page hoặc escalation. Giải thích và kiểm chứng cơ chế, index và bất biến dữ liệu; gợi ý khóa phải đi cùng transaction/ràng buộc đúng.



**### Luồng giữ chỗ phải thực thi được**



- Server xác thực người gọi và chuyến có cho phép đặt không.

- Xử lý transaction ngắn, khóa các ghế theo thứ tự ổn định nếu có nhiều ghế.

- Kiểm tra trạng thái hiện tại sau khi đã lấy khóa.

- Hold đã hết hạn được kiểm tra và thu hồi nguyên tử; không phụ thuộc duy nhất vào worker đã chạy hay chưa.

- Nếu ghế có thể giữ, ghi Held + chủ sở hữu + định danh hold + ExpiresAt, rồi commit.

- Nếu ghế thuộc người khác hoặc đã đặt, trả xung đột nghiệp vụ phù hợp; không tạo hold hoặc vé trùng.

- Chỉ sau commit mới phát thông báo SignalR và trả thành công.



Có thể dùng thời gian SQL Server như \`SYSUTCDATETIME()\` và phép cộng 5 phút phù hợp. Không dùng giá trị “now” lấy trước một lần chờ khóa dài để xác nhận một hold đã hết hạn ở thời điểm quyết định cập nhật.



**### Ownership và token**



- Mỗi hold có định danh/token không thể dự đoán hợp lý; không dùng mỗi SeatId làm bằng chứng sở hữu.

- Các thao tác xác nhận, trả chỗ hoặc cập nhật hold phải kiểm người dùng, hold hiện tại và hạn sử dụng.

- Token cũ của A không thể xác nhận/giải phóng ghế sau khi hold A đã hết hạn và ghế đã chuyển cho B.

- Không tự gia hạn vô hạn bằng refresh hoặc gửi lại request.

- Có xử lý idempotency cho các thao tác quan trọng để double-click/retry sau timeout không tạo hai đơn/vé.

- Không broadcast token, UserId người giữ, số điện thoại hoặc thông tin hành khách tới nhóm SignalR công khai.



**### Xác nhận đặt vé**



Trong transaction ngắn: xác minh đúng owner/hold, chuyến còn hợp lệ và hold chưa hết hạn; tạo đơn/vé và chuyển Held → Booked một cách nguyên tử. Điều kiện thời hạn phải được kiểm ở điểm cập nhật có thẩm quyền, không chỉ kiểm trên UI.



Nếu bất kỳ bước nào thất bại, rollback toàn bộ. Ràng buộc database ngăn hai vé hiệu lực cho một ghế chuyến. Gửi lại cùng một thao tác thành công phải nhận kết quả nhất quán, không ghi thêm vé.



Các luồng hủy chuyến, đóng bán, worker hết hạn, bán tại quầy và đặt online phải phối hợp cùng quy tắc giao dịch để không bán vé cho chuyến không hợp lệ.



**## 9. Hết hạn, nhả ghế và khôi phục sau restart**



Triển khai BackgroundService/IHostedService:



- Quét hold hết hạn theo batch và index, khoảng quét được cấu hình/ghi rõ.

- Mỗi cập nhật dùng transaction ngắn và điều kiện trạng thái/hold hiện tại.

- Chỉ giải phóng hold còn đúng định danh và thực sự hết hạn; không ghi đè hold mới hoặc ghế Booked.

- Sau commit mới phát sự kiện cập nhật ghế.

- Lỗi worker phải được log; không làm ứng dụng báo thành công giả hoặc âm thầm mất khả năng thu hồi.



Worker không phải lớp bảo vệ duy nhất. API giữ/xác nhận cũng phải tự kiểm hạn trong database. Khi app/worker dừng rồi chạy lại, hạn giữ không bị đặt lại; hold quá hạn được xử lý và người dùng không bị kẹt ghế mãi.



Countdown dùng \`ExpiresAt\` và thời gian server trả về. Countdown về 0 thì đồng bộ lại với server; trình duyệt không tự quyết định ai sở hữu ghế.



Hủy vé/đổi vé phải cập nhật ghế và vé liên quan trong cùng transaction thích hợp, giữ lịch sử. Nếu đổi thất bại, vé/ghế cũ không được mất hiệu lực một nửa. Kiểm cả tranh chấp khi nhân viên bán/đổi vé cùng lúc khách đặt online.



**## 10. SignalR chỉ đồng bộ giao diện**



- SQL Server là nguồn quyết định trạng thái và quyền sở hữu ghế.

- SignalR Hub không tự xác nhận ghế dựa trên dữ liệu client hoặc bộ nhớ RAM.

- Client tham gia nhóm theo TripId hợp lệ; không đưa thông tin đặt vé riêng tư vào nhóm công khai.

- Chỉ gửi trạng thái cần thiết của ghế và version/revision/thời điểm phù hợp sau commit.

- Có cơ chế nhận biết sự kiện cũ hoặc lệch thứ tự để không làm UI quay về trạng thái trước.

- Khi reconnect, chuyển tab trở lại hoặc nghi ngờ mất sự kiện, tải snapshot trạng thái từ server rồi đối chiếu với sự kiện mới.

- Xử lý được gửi sự kiện thất bại sau commit: dữ liệu đã commit vẫn đúng, log lỗi và cho client đồng bộ lại; không báo rollback SQL khi SQL đã commit.

- Mất SignalR, mất WebSocket, client chậm hoặc refresh không được làm phát sinh double booking.

- Có trạng thái kết nối trên UI; khi trạng thái cục bộ cũ, server vẫn kiểm tra lại khi người dùng giữ/xác nhận.



Không thêm chat realtime. Real-time ở đề tài này phục vụ trạng thái ghế/chuyến trong phạm vi nghiệp vụ đã đăng ký.



**## 11. Authentication và bốn vai trò**



Dùng ASP.NET Core Identity; các tài khoản mẫu tạo bằng UserManager/RoleManager, không INSERT PasswordHash thủ công, không lưu mật khẩu plaintext trong DB.



**### Khách chưa đăng nhập**



Xem thông tin tuyến/chuyến, tìm chuyến, xem ghế; đăng ký/đăng nhập. Mặc định yêu cầu đăng nhập trước khi giữ chỗ/đặt vé để kiểm ownership rõ ràng. Guest không phải vai trò quản trị thứ năm.



**### Khách hàng — Customer**



- Quản lý thông tin tài khoản cơ bản.

- Tra cứu chuyến, xem ghế, giữ/nhả chỗ của mình.

- Điền thông tin hành khách và xác nhận đặt vé.

- Xem mã vé, chi tiết, lịch sử đặt vé và trạng thái thanh toán của mình.

- Thực hiện yêu cầu hủy/đổi trong phạm vi chính sách đã triển khai; không sửa vé của người khác.



**### Nhân viên — Staff**



- Quản lý và xử lý đặt vé theo quyền.

- Hỗ trợ bán tại phòng vé, đổi/trả và tra cứu vé.

- Xác nhận đã thu tiền trực tiếp; ghi nhận xử lý tiền liên quan theo chính sách.

- Xem danh sách hành khách theo chuyến.

- Không tự cấp Admin hoặc bỏ qua chống trùng bằng cách cập nhật ghế tùy tiện.



**### Tài xế — Driver**



- Xem lịch phân công cá nhân.

- Xem danh sách hành khách và vị trí ghế trên chuyến được giao.

- Cập nhật tiến trình chuyến hợp lệ: chuẩn bị chạy → đang chạy → hoàn thành.

- Không quản lý tài khoản, sửa doanh thu hoặc truy cập hành khách của mọi chuyến bằng cách đổi ID trên URL.



**### Quản trị viên — Admin**



- Quản lý người dùng và phân quyền bốn vai trò.

- Quản lý tuyến, xe, sơ đồ ghế, chuyến, giá và phân công.

- Theo dõi vé, vận hành và thống kê toàn hệ thống.

- Quản lý trạng thái hoạt động của danh mục, không xóa mất lịch sử vé.



Phải có ma trận quyền và kiểm authorization/ownership ở server, không chỉ ẩn menu. Bảo vệ POST/API, form anti-forgery, CSRF/XSS và IDOR phù hợp. SQL dùng tham số, không ghép chuỗi dữ liệu người dùng. Hub cũng có kiểm quyền phù hợp.



Thông tin hành khách chỉ lộ cho chủ vé/người tác nghiệp được phép, không trả danh sách PII qua API sơ đồ ghế công khai.



**## 12. Các module và trang bắt buộc**



Tên route có thể theo conventions nhưng chức năng phải có thật.



**### Public và Khách hàng**



- Trang chủ có form tìm chuyến hoạt động.

- Danh sách kết quả theo điểm đi/đến/ngày, thông tin xe/giờ/giá/ghế còn phù hợp.

- Chi tiết chuyến và sơ đồ ghế tương tác.

- Giữ chỗ, countdown 5 phút, thông tin hành khách, xác nhận đặt vé.

- Kết quả đặt vé/mã vé, chi tiết và lịch sử cá nhân.

- Đăng ký, đăng nhập, đăng xuất, hồ sơ cơ bản.

- Trạng thái không có chuyến, hết ghế, hold hết hạn, xung đột và mất kết nối rõ ràng.



**### Admin**



Dashboard; quản lý người dùng/role; tuyến; xe; sơ đồ ghế/ghế; chuyến và giá; phân công tài xế/nhân viên; theo dõi vé và báo cáo.



**### Nhân viên**



Danh sách/tra cứu đặt vé; chi tiết; xác nhận thanh toán trực tiếp; bán vé hỗ trợ; đổi/trả theo quy định; danh sách hành khách theo chuyến.



**### Tài xế**



Chuyến được giao; chi tiết chuyến/danh sách hành khách/ghế; cập nhật trạng thái được phép.



Không tạo menu dẫn tới route chưa triển khai. Không có nút chỉ hiện “thành công” mà database không thay đổi. Form sai phải có validation, không thành lỗi 500.



**## 13. Dashboard và thống kê đúng nghiệp vụ**



Có KPI và bộ lọc thời gian phù hợp:



- Số chuyến, vé đã đặt/còn hiệu lực/hủy.

- Số vé đã thu tiền/chưa thu tiền.

- Doanh thu theo kỳ, tuyến hoặc chuyến trong phạm vi dữ liệu đang có.

- Biểu đồ Chart.js cần thiết để đọc số liệu.



Giải thích công thức: giữ chỗ chưa phải vé đã bán; vé chưa thu tiền không tự thành doanh thu đã thu; vé hủy/đổi/hoàn tiền phải có cách phản ánh nhất quán. Tránh cộng hai lần khi retry hoặc đổi vé. Số liệu phải lấy từ SQL Server, không gán số cứng trong View/JavaScript.



**## 14. Giao diện, hình ảnh và trải nghiệm**



Tham khảo ZIP về chất lượng hoàn thiện, CSS/JS local, responsive và tổ chức trang. Thiết kế riêng đúng **\*\*dịch vụ xe khách/đặt vé\*\***, không tái sử dụng thương hiệu Esports/Riot.



- Giao diện tiếng Việt, dễ đọc, hiện đại; màu sắc, typography, khoảng cách và trạng thái thống nhất bằng CSS variables.

- Trang chủ tìm chuyến, kết quả, trang chọn ghế và đặt vé phải rõ ràng hơn trang quảng cáo hình thức.

- Sơ đồ ghế đúng cấu hình dữ liệu; có mã ghế, hàng/tầng nếu dùng và chú giải Available/Held/Booked/đang chọn/của tôi.

- Trạng thái có nhãn/icon, không chỉ dựa vào màu.

- Phân biệt “đang chọn trên UI” và “đã giữ thành công tại server”. Chưa được server chấp nhận thì không hiển thị như đã sở hữu.

- Countdown và cảnh báo hết hạn dễ hiểu; không gây hiểu nhầm ghế Booked là ghế còn chọn được.

- Loading/disabled/chống gửi trùng khi request; thông báo phản ánh đúng kết quả server, xung đột thì tải lại trạng thái.

- Animation nhẹ, IntersectionObserver nếu có; hỗ trợ prefers-reduced-motion, focus/keyboard, form label và alt.

- Kiểm 375px, 768px, 1024px, 1440px. Sơ đồ ghế và bảng tác nghiệp không tràn hoặc mất nút.

- Hình xe/tuyến/chủ đề vận tải phải có quyền phù hợp và lưu local; ưu tiên ảnh ngang phù hợp khung. Không hotlink tạm, kéo méo, xóa watermark, giả ảnh sự kiện/nhà xe chính thức.

- Ghi ASSET_SOURCES.md. Nội dung tuyến/giá/lịch seed phải rõ dữ liệu thử nghiệm, không giả là lịch bán vé thật của doanh nghiệp.



Không dùng skeleton giả cho nội dung Razor đã render, không dùng thư viện animation nặng chỉ để trang trí, không thêm UI giả cho tính năng chưa làm.



**## 15. SQL Server, migration, seed và hướng dẫn chạy**



Website dùng SQL Server làm database nghiệp vụ chính ngay từ đầu.



- Connection string qua config/User Secrets/biến môi trường; không hard-code mật khẩu production hoặc server máy tôi.

- Hướng dẫn SSMS để xác định đúng Server name, database và quyền kết nối.

- Có migration tương ứng model, tạo bảng/constraint/index và Identity đồng bộ.

- Chỉ dùng một quy trình tạo schema rõ ràng. Không chạy EnsureCreated rồi Migrate trên cùng DB; không chạy SQL Legacy Esports hoặc script schema chồng lên migration.

- Nếu xuất SQL từ migration, ghi rõ đây là phương án triển khai thay thế hoặc bước review, không bắt thực thi cả hai cách lên cùng schema.

- Không tự xóa DB, giả migration history hoặc ghi đè dữ liệu người dùng khi startup.

- Seed học tập chỉ chạy có kiểm soát trên môi trường development/test, có tính idempotent. Không tạo bản ghi trùng khi restart.

- Dữ liệu mẫu đủ kiểm tra nhiều tuyến/chuyến/xe/sơ đồ, bốn vai trò và danh sách hành khách. Có chuyến còn mở bán trong thời gian chạy thử, không chỉ seed ngày đã qua.

- Không đưa thông tin cá nhân thật, mật khẩu thật, API key hoặc file DB riêng vào ZIP.



README tiếng Việt phải có đủ bước: cài workload/SDK theo .NET 9, mở solution, Restore, cấu hình SQL Server qua SSMS/User Secrets, chạy migration, seed, chọn startup project/profile, F5 và URL thực tế. Cung cấp các lệnh chính xác theo solution đã tạo; không bắt tôi đoán tham số hay instance.



Nếu SQL Server hoặc .NET không sẵn trong môi trường làm việc của bạn, vẫn hoàn thiện source/config/tài liệu và bộ test, ghi rõ chưa kiểm chứng. Không dùng SQLite hoặc dữ liệu RAM để thay cho việc kiểm bài toán khóa SQL Server.



**## 16. Kiểm thử chức năng, lỗi và tính đúng đắn**



Thực hiện khi môi trường cho phép:



1. \`dotnet --info\` và xác minh SDK/runtime được chọn.

2. \`dotnet restore\`, \`dotnet build\`.

3. Khởi tạo SQL Server test theo migration và seed có kiểm soát.

4. \`dotnet run\`.

5. Kiểm HTTP/Controller/API/Razor/DI/database, bốn vai trò và các trang hợp lệ.

6. Kiểm qua browser nếu có công cụ; không chỉ kiểm cú pháp JS rồi báo UI đạt.



Test nghiệp vụ tối thiểu:



- Ghế chuyến độc lập giữa hai chuyến dùng cùng xe/vị trí.

- Available → Held → Booked.

- Held → Available khi hết hạn; reload/reconnect không gia hạn.

- Hold còn hiệu lực qua restart, hold quá hạn được thu hồi sau restart.

- Token sai, sai owner, token cũ sau khi người khác giữ lại.

- Xác nhận sát/đúng/sau hạn; đua giữa expiry worker và xác nhận.

- Double-click/retry giữ chỗ/xác nhận/hủy; không tạo trùng đơn/vé.

- Nhiều ghế: thành công toàn bộ hoặc rollback toàn bộ.

- Hủy vé và đổi vé giải phóng/chuyển ghế đúng, giữ lịch sử, không có trạng thái nửa chừng.

- Khách online tranh chấp với nhân viên bán tại quầy.

- Hủy/đóng chuyến đồng thời với giữ/xác nhận.

- Người không đăng nhập, sai role, thay ID để truy cập vé/chuyến của người khác.

- Request/form sai, giá giả, chuyến hết ghế/không tồn tại/đã khởi hành/không mở bán.

- SQL Server chưa cấu hình, thiếu migration, thiếu quyền; ảnh local thiếu có fallback.

- Dashboard đúng dữ liệu thu tiền/hủy/đổi.



Kiểm ít nhất một luồng 5 phút thực với cấu hình mặc định 300 giây. Có thể dùng dữ liệu fixture hết hạn hoặc công cụ thời gian kiểm soát cho các test nhanh, nhưng ghi rõ phương pháp; không coi test rút ngắn vài giây là bằng chứng duy nhất rằng cấu hình 5 phút đã chạy đúng.



HTTP status phải phản ánh kết quả: thành công hợp lệ, 400/validation, 401/403, 404 và 409 cho xung đột nghiệp vụ theo thiết kế. Không để route hợp lệ/validation thông thường thành 500; cũng không catch mọi lỗi rồi trả 200. Lỗi bất ngờ có log/request ID và thông báo không lộ bí mật.



**## 17. Kiểm thử concurrency bắt buộc theo giảng viên**



Phải có script chạy được trên **\*\*SQL Server thật\*\***, bằng công cụ phù hợp. Không coi EF InMemory, SQLite hoặc test một request lần lượt là bằng chứng tương đương.



Các mức **\*\*20, 50 và 100 yêu cầu đồng thời\*\*** cùng tranh một ghế chuyến:



- Nhiều người dùng/phiên độc lập, request hợp lệ, có điểm khởi phát đồng thời và thời hạn hold chưa trôi qua trong đợt tranh chấp.

- Mỗi đợt bắt đầu từ điều kiện dữ liệu xác định; có thể lặp nhiều đợt với dữ liệu test riêng, không reset DB người dùng.

- Chỉ một yêu cầu logic mới được giành quyền giữ/đặt ghế; request còn lại bị từ chối đúng lý do xung đột.

- Đối chiếu trực tiếp DB sau test: một chủ giữ hợp lệ hoặc một vé hiệu lực, không chỉ nhìn số toast/HTTP 200.

- Double booking bằng **\*\*0\*\*** ở tất cả các đợt đã chạy.

- Phân biệt retry idempotent của một thao tác với một giao dịch sở hữu ghế mới; không đếm sai người thắng.

- Không tính 401, lỗi anti-forgery, payload sai hoặc server 500 là “chống trùng thành công”.



Kiểm thêm nhiều client xem cùng chuyến, đua confirm/cancel/expire, lỗi giữa transaction và retry. Thử SignalR chậm/ngắt trong khi các request vẫn tranh ghế; DB vẫn phải đúng. Nếu chưa chạy được mức tải nào thì ghi CHƯA KIỂM CHỨNG, không tự điền số đẹp.



**## 18. Đánh giá định lượng bắt buộc**



Tạo báo cáo có số liệu đo thực tế:



- Mức tải và số request đồng thời.

- Số request gửi/hoàn thành, người thắng mới, xung đột hợp lệ và lỗi bất ngờ.

- Response time trung bình và P95; ghi đơn vị, tập mẫu và cách tính.

- Tỷ lệ thành công kỹ thuật/xung đột nghiệp vụ được định nghĩa rõ; việc một người thắng trên 100 người tranh cùng ghế không tự là tỷ lệ lỗi 99%.

- Số active hold/vé hiệu lực ở DB sau test và số double booking.

- Thời gian đồng bộ trạng thái từ commit SQL đến client qua SignalR: có phương pháp đo, số mẫu và điều kiện mạng. Không trừ timestamp của hai máy lệch đồng hồ để báo latency sai; phân biệt đo một chiều với ACK khứ hồi.

- Hành vi khi disconnect/reconnect, độ trễ và cách client khôi phục snapshot.

- Cấu hình máy, phiên bản SQL/.NET, số instance app, công cụ và ngày chạy test.



Có bảng riêng cho 20/50/100 request. Giữ raw logs/CSV/JSON đủ đối chiếu. Không tự bịa ngưỡng P95 hoặc tuyên bố đáp ứng một SLA mà đề cương chưa yêu cầu. Báo cáo số đo và giới hạn trung thực.



**## 19. Tài liệu bàn giao gắn với yêu cầu giảng viên**



Tài liệu Markdown trong ZIP tối thiểu:



1. \`README.md\`: cách mở/chạy thật với SQL Server, tài khoản test development và xử lý lỗi phổ biến.

2. \`REQUIREMENTS_TRACEABILITY.md\`: từng yêu cầu Word/ảnh → module → file/API → testcase → bằng chứng/kết quả.

3. \`DATABASE_DESIGN.md\` và \`ERD.md\`: thực thể, khóa, quan hệ, index, constraint và phân biệt ghế vật lý/ghế chuyến.

4. \`USE_CASE.md\`, \`ACTIVITY_DIAGRAMS.md\`, \`SEQUENCE_DIAGRAMS.md\`: bốn vai trò; giữ/xác nhận/hết hạn/hủy/đổi và tranh chấp.

5. \`SEAT_STATE_MACHINE.md\`: trạng thái, điều kiện chuyển, actor và bất biến.

6. \`HOLD_ALGORITHM.md\`: transaction ngắn, SQL/EF thực tế, vị trí khóa/commit, thời gian server, ownership, idempotency và xử lý tranh chấp.

7. \`REALTIME_DESIGN.md\`: SignalR sau commit, event payload/version, reconnect và mất kết nối.

8. \`PERMISSION_MATRIX.md\`: quyền theo role và ownership.

9. \`SQL_SERVER_SETUP.md\`: SSMS, connection string, migration/seed và backup.

10. \`ASSET_SOURCES.md\`: nguồn/quyền các ảnh và thư viện.

11. \`TEST_RESULTS.md\`, \`CONCURRENCY_TEST_REPORT.md\`, \`PERFORMANCE_REPORT.md\`: PASS/FAIL/CHƯA KIỂM CHỨNG và log.

12. \`LIMITATIONS.md\` và hướng dẫn giải thích kỹ thuật khi bảo vệ.



Có thể dùng Mermaid cho sơ đồ. Tài liệu mô tả implementation thực tế, không mô tả chức năng chưa làm như đã hoàn tất. Đây là tài liệu kỹ thuật kèm website, không phải yêu cầu tự viết lại đề cương hoặc toàn bộ báo cáo Word trong giai đoạn này.



Bảng đối chiếu phải thể hiện đủ các điểm giảng viên nêu:



\| Điểm bắt buộc                                      | Bằng chứng cần có                                              |

\| -------------------------------------------------- | -------------------------------------------------------------- |

\| Tuyến – Chuyến – Xe – Sơ đồ ghế – Ghế chuyến – Vé  | ERD và dữ liệu thử nghiệm hợp lý                               |

\| Luồng trạng thái ghế/vé, hết hạn, hủy nhả ghế      | State machine, service và testcase                             |

\| Giữ 5 phút bằng hạn lưu bền vững, transaction ngắn | Schema ExpiresAt, SQL/EF và log kiểm thử                       |

\| Chống double booking tại DB                        | Constraint/transaction + test 20/50/100 concurrent             |

\| SignalR/WebSocket chỉ đồng bộ, phát sau commit     | Kiến trúc, code và test chậm/mất kết nối                       |

\| Đánh giá định lượng                                | Response time/P95, concurrency, double booking, độ trễ đồng bộ |



**## 20. Quy trình thực hiện và điều kiện nghiệm thu**



Thực hiện tuần tự nhưng không dừng lại ở kế hoạch:



1. Đọc Word/ảnh, lập phạm vi và traceability, rà ZIP tham khảo đúng mục đích.

2. Thiết kế entity/ERD/state machine và quyền.

3. Tạo solution .NET 9, SQL Server schema/migrations/Identity/seed.

4. Hoàn thiện service giữ chỗ, expiry, xác nhận/hủy/đổi và ràng buộc.

5. Hoàn thiện bốn vai trò và các module vận hành.

6. Tích hợp SignalR, snapshot/reconnect và UI sơ đồ ghế.

7. Hoàn thiện CSS/JS/ảnh, dashboard.

8. Restore/build/run, test SQL Server và các route/tương tác; sửa lỗi tìm được.

9. Chạy test concurrency/đo hiệu năng, hoàn thiện tài liệu và đóng gói.



Ưu tiên tính đúng đắn của giữ chỗ/chống trùng hơn trang trí. Không bỏ một chức năng đã đăng ký để thay bằng trang đẹp. Không kết luận “đã hết lỗi 500”, “SQL Server chạy được”, “double booking bằng 0” hoặc “đã real-time” khi chưa có bằng chứng tương ứng.



Nếu môi trường không có SQL Server/.NET/browser, ghi rõ phần bị chặn và những lệnh đã thử. Không biến kiểm tra cú pháp, xem source hoặc test DDL giả lập thành kết quả chạy ứng dụng.



**## 21. Đóng gói ZIP và kết quả cuối**



Bàn giao **\*\*một ZIP source chính\*\***, ví dụ:



\`QuanLyXeKhach_DatVe_GiuCho_REALTIME_REVIEWED.zip\`



Chỉ dùng hậu tố \`VERIFIED\` nếu đã restore/build/run thành công trên SQL Server và kiểm chứng các luồng cốt lõi, bao gồm giữ chỗ/ownership/hết hạn/chống trùng và bốn vai trò. Nếu còn kiểm tra chưa chạy thì ghi rõ, không dùng tên VERIFIED chỉ vì trang chủ mở được.



ZIP có solution/project, Controllers, Models, DbContext, ViewModels, Services, Hubs, BackgroundServices, Razor Views, Areas/giao diện vai trò, SQL migrations/setup, seed test, toàn bộ CSS/JS/thư viện/ảnh local, test scripts và tài liệu.



Không đóng gói \`bin\`, \`obj\`, \`.vs\`, cache, \`node_modules\`, file DB cá nhân, production credentials, API key, SQL backup riêng, ZIP trung gian hoặc dữ liệu hành khách thật.



Khi hoàn tất, gửi:



- Link tải ZIP và phiên bản .NET/Visual Studio/SQL Server đã dùng hoặc đã kiểm tra.

- Danh sách chức năng thực sự có và các giới hạn còn lại.

- Hướng dẫn mở solution, kết nối SQL Server, migrate/seed và chạy.

- Tài khoản test của bốn vai trò trong Development.

- Báo cáo build, route/HTTP, workflow và ma trận quyền.

- Bảng test giữ 5 phút, expiry, restart, ownership và 20/50/100 request tranh ghế.

- Số liệu P95/thời gian phản hồi/độ trễ SignalR đo được; không bịa số.

- PASS/FAIL/CHƯA KIỂM CHỨNG cho từng nhóm, kèm vị trí bằng chứng.

- Xác nhận phạm vi vẫn đúng đề cương và ảnh nhận xét; mọi khác biệt phải được liệt kê rõ.



**\*\*Nhắc lại: triển khai đúng nguyên đề tài xe khách đã đăng ký, với SQL Server là nơi quyết định quyền sở hữu ghế, giữ chỗ bằng ExpiresAt 5 phút và transaction ngắn, SignalR chỉ đồng bộ sau commit. Không chuyển thành Esports Portal, không thêm Demo SQLite/Preview, không đổi framework hoặc phạm vi đã chốt.\*\***
