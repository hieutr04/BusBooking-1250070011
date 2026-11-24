**# NÂNG CẤP BUSBOOKING: ẢNH XE KHÁCH THẬT, GIAO DIỆN TRỰC QUAN VÀ TRANG LIÊN HỆ**



**## 1. Giữ nguyên đề tài, chuyên ngành và hệ thống đang chạy**



Tôi đang làm đồ án chuyên ngành **\*\*Thương mại điện tử\*\***.



Tên đề tài chính thức phải giữ nguyên:



**\*\*XÂY DỰNG HỆ THỐNG QUẢN LÝ XE KHÁCH VÀ ĐẶT VÉ TRỰC TUYẾN TÍCH HỢP CƠ CHẾ GIỮ CHỖ THEO THỜI GIAN THỰC\*\***



Tôi đính kèm source mới nhất:



\`QuanLyXeKhach_DatVe_GiuCho_REALTIME_UI_UPGRADED_REVIEWED(1).zip\`



và các ảnh chụp website đang chạy, trong đó có ảnh trang Liên hệ FUTA để tham khảo bố cục.



**\*\*Website trên máy tôi đã kết nối SQL Server thành công. Hãy giữ cấu hình và dữ liệu hiện có, sửa trực tiếp project này, không xây lại website, không đổi tên đề tài, không đổi chuyên ngành, không thay nghiệp vụ hoặc các yêu cầu đã xác lập.\*\***



Lần này chỉ bổ sung ba nhóm việc:



1. Tìm và tích hợp **\*\*ảnh xe khách thật, ảnh hành trình/điểm đến phù hợp\*\***, có nguồn và quyền sử dụng xác minh được, thay ảnh minh họa chính đang quá đơn điệu.

2. Cải thiện phần trình bày giao diện để website thực tế, trực quan, thu hút và dễ đặt vé hơn; tiếp tục gam **\*\*trắng và xanh\*\***.

3. Thêm **\*\*trang Liên hệ\*\*** tham khảo cách tổ chức của FUTA, với biểu mẫu hoạt động thật và phần tiếp nhận tối thiểu trong Admin.



**\*\*Hãy thực hiện thay đổi source, kiểm tra và gửi ZIP source mới. Không chỉ đưa hướng dẫn, mockup, code mẫu hoặc một bản báo cáo.\*\***



**## 2. Đọc source mới trước khi sửa**



Source đầu vào hiện có:



- \`BusBooking.sln\`, project \`BusBooking/BusBooking.csproj\`, target \`net9.0\`.

- ASP.NET Core MVC/Razor, Identity, EF Core SQL Server, migrations và snapshot.

- Bốn vai trò **\*\*Admin, Staff, Driver, Customer\*\***.

- Các chức năng tuyến, xe, sơ đồ ghế, chuyến/phân công, tìm chuyến, giữ chỗ, đặt vé, đổi/hủy, thu/hoàn trực tiếp, hành khách, thống kê, nhật ký.

- Worker hết hạn hold, \`BookingService\`, SignalR Hub và JavaScript đồng bộ ghế.

- Layout khách hàng và \`\_AdminLayout.cshtml\`, sidebar/workspace đã có từ lần nâng cấp trước.

- CSS \`site.css\`, \`ui.css\`, \`admin.css\`; JS \`site.js\`, \`seats.js\`, \`dashboard.js\`, \`admin.js\`.

- Ảnh chính hiện tại: \`wwwroot/images/bus-journey.svg\`, \`bus-small.svg\`, \`landscape.svg\`; favicon và icon riêng.

- \`Views/Home/Index.cshtml\` đang dùng SVG xe cho hero và cùng một SVG phong cảnh cho các card tuyến.

- Hồ sơ yêu cầu, nguồn tài sản và kiểm thử trong ZIP; chưa thấy chức năng Liên hệ trong bản này.



Đây là mô tả bản ZIP hiện tại để định hướng; vẫn phải đọc file thực tế, vì bản tôi gửi có thể đã thay đổi. Kiểm tra riêng logic chuyển hướng đăng nhập theo vai trò, không làm mất bản sửa Admin đăng nhập vào khu quản trị.



Đọc tài liệu yêu cầu gốc và nghiệp vụ trong ZIP. Nếu Word/ảnh nhận xét giảng viên cũ không được đính kèm lại thì nói rõ chưa trực tiếp đối chiếu được, không giả vờ đã đọc. Không sửa file yêu cầu gốc hoặc tự loại bỏ yêu cầu cũ.



**## 3. Những thành phần bắt buộc giữ nguyên**



- Tên kỹ thuật **\*\*BusBooking\*\***, thương hiệu giao diện **\*\*véxe.\*\***, tên đề tài đầy đủ và chuyên ngành Thương mại điện tử.

- ASP.NET Core MVC **\*\*.NET 9\*\***, Razor, Identity, EF Core SQL Server, Bootstrap 5, SignalR, Chart.js.

- \`TargetFramework=net9.0\`, cấu hình SDK .NET 9 phù hợp hiện tại; không chuyển sang .NET 8 hoặc .NET 10.

- Solution/project, Controller/action/route, ViewModel và service đang hoạt động, trừ bổ sung tối thiểu cho ảnh/Liên hệ.

- Database SQL Server, migrations/snapshot, Identity, tài khoản, role và dữ liệu nghiệp vụ hiện có.

- Quy tắc tính tiền, giá chốt, thu/hoàn trực tiếp, số liệu dashboard và mốc thời gian hiện tại.

- Form method/action, model binding, validation, anti-forgery, authorization và ownership.

- Redirect đăng nhập: Admin vào quản trị; Staff/Driver vào workspace phù hợp; Customer tiếp tục luồng đặt vé qua returnUrl nội bộ an toàn.

- CSS/JS/animation, mobile menu, sidebar, loading và các chức năng đã được làm từ bản nâng UI.



Không tự cài lại SQL Server, thay connection string của tôi, đặt lại mật khẩu, reset/seed lại dữ liệu, xóa database, tạo Initial migration mới hoặc sửa migration history chỉ vì thay giao diện.



**\*\*Không mang Demo SQLite, InMemory, JSON backend hoặc fallback giả vào project.\*\*** Không mở hosting/tunnel/firewall, không triển khai công khai trong nhiệm vụ này.



**## 4. Tìm ảnh trên Google và xác minh nguồn gốc**



Sử dụng công cụ tìm kiếm/trình duyệt được hỗ trợ. Ưu tiên Google Search/Google Images để khám phá; nếu công cụ không cho chọn Google trực tiếp, dùng công cụ web được hỗ trợ và nói đúng công cụ/phạm vi đã sử dụng.



Gợi ý từ khóa:



- “xe khách đường dài”, “xe khách giường nằm”, “xe khách ghế ngồi”.

- “coach bus highway photo”, “tourist coach bus stock photo”.

- “Ho Chi Minh City skyline”, “Da Lat landscape”, “Nha Trang coast” và ảnh đúng điểm đến thực tế có trong dữ liệu.



**\*\*Ảnh xuất hiện trên Google không có nghĩa được phép sao chép.\*\*** Luôn mở trang gốc của từng ảnh, xác minh tác giả/chủ sở hữu và giấy phép trước khi tải vào source.



Có thể tìm trên Pexels, Unsplash, Wikimedia Commons hoặc nguồn có giấy phép rõ ràng. Kiểm điều kiện của **\*\*đúng ảnh và giấy phép hiện hành\*\***, không mặc định toàn bộ ảnh ở một website đều dùng được. Với CC BY/CC BY-SA phải thực hiện yêu cầu ghi công, link giấy phép và ghi nhận chỉnh sửa/chia sẻ thích hợp nếu áp dụng.



Không dùng:



- Ảnh không rõ nguồn/quyền; ảnh có watermark; ảnh chụp từ Google Images hoặc URL thumbnail Google.

- URL \`googleusercontent\`, hotlink tạm hoặc chỉ lưu link ảnh ngoài vào database.

- Ảnh của FUTA/nhà xe khác chỉ vì có trên website chính thức, khi chưa có quyền dùng lại.

- Ảnh xe buýt đô thị, limousine con hoặc xe khác loại rồi mô tả là xe giường nằm đang bán vé.

- Ảnh có thương hiệu bên thứ ba nổi bật khiến người xem tưởng véxe. là đại lý/chủ sở hữu/đối tác của họ. Không xóa logo/watermark để né điều kiện sử dụng; chọn ảnh khác phù hợp.

- Ảnh AI rồi báo đó là ảnh chụp xe thật.



Ưu tiên ảnh xe khách đường dài rõ nét, bố cục ngang, chủ thể không bị cắt, ít chi tiết thương hiệu bên thứ ba. Ảnh minh họa phải được ghi rõ là **\*\*ảnh minh họa\*\***, không khẳng định đó là xe, nội thất hoặc dịch vụ thật của một chuyến trong dữ liệu học tập.



Nếu dùng ảnh điểm đến, ảnh Đà Lạt phải đúng Đà Lạt, ảnh Nha Trang phải đúng Nha Trang, ảnh TP. Hồ Chí Minh phải đúng TP. Hồ Chí Minh; kiểm nguồn/chú thích thay vì suy đoán từ hình. Không dùng cùng một phong cảnh không rõ địa điểm cho mọi tuyến rồi gắn tên địa phương khác nhau.



Hướng tới một bộ ảnh gọn khoảng **\*\*6–10 ảnh phù hợp nếu tìm được nguồn hợp lệ\*\***, ưu tiên chất lượng và quyền sử dụng hơn số lượng. Nếu không đủ, báo rõ; không đưa ảnh chưa xác minh vào ZIP để đạt chỉ tiêu.



**## 5. Tải ảnh local, quản lý đường dẫn và ghi nguồn**



Ảnh được chọn phải tải thật vào project, ví dụ:



\`\`\`text

wwwroot/images/

  buses/

    coach-hero.webp

    coach-exterior.webp

  destinations/

    ho-chi-minh-city.webp

    da-lat.webp

    nha-trang.webp

  banners/

    contact-journey.webp

  placeholders/

    bus-fallback.svg

\`\`\`



Tên file trên chỉ là cấu trúc gợi ý; không tạo tên giả hoặc khẳng định file đã tồn tại khi chưa tải. Ưu tiên WebP/JPG; PNG khi cần. Chọn độ phân giải phù hợp, tối ưu dung lượng, tránh đưa ảnh gốc hàng chục MB lên card nhỏ.



- Có width/height hoặc aspect-ratio để tránh layout shift.

- Hero/card/banner ưu tiên bố cục ngang, thường 16:9; cho phép tỷ lệ khác nếu phù hợp và giữ chủ thể đầy đủ.

- Dùng \`object-fit\`, \`object-position\` theo ảnh; không kéo giãn hoặc crop mất đầu/đuôi xe, điểm nhận diện quan trọng.

- Alt tiếng Việt đúng nội dung; ghi công/chú thích tại vị trí phù hợp khi giấy phép yêu cầu. Alt không thay thế nghĩa vụ ghi công.

- Ảnh dưới màn hình đầu dùng lazy loading; hero không bị lazy-load không cần thiết.

- Fallback local khi thiếu file, tránh vòng lặp lỗi ảnh; giữ icon SVG/favicon hợp lệ đang có.

- Website vẫn hiển thị ảnh khi localhost mất Internet.



Nếu chỉ thay ảnh trình bày, ưu tiên Razor/config mapping/view model có fallback, **\*\*không đổi schema các bảng tuyến/xe/chuyến để lưu ảnh khi chưa cần\*\***. Không hard-code toàn hệ thống chỉ có ba địa điểm hoặc chỉ map ID seed rồi làm ảnh sai ở database khác. Dữ liệu mới chưa có ảnh phải có fallback trung tính và alt phù hợp.



Nếu gắn ảnh xe với loại xe, chỉ dùng phân loại rõ ràng từ dữ liệu hiện có; không suy đoán biển số hoặc tự gán ảnh xe của nhà xe khác thành xe thật trong hệ thống. Ảnh ngoại thất chung có thể dùng như minh họa chung với chú thích rõ.



Cập nhật \`ASSET_SOURCES.md\`, mỗi ảnh có:



\| Trường | Nội dung bắt buộc |

\|---|---|

\| File và đường dẫn | File local thực sự trong source |

\| Nội dung | Xe/hành trình/địa điểm, ảnh chụp hay minh họa |

\| Nguồn | URL trang gốc ảnh, tác giả/chủ sở hữu, URL tải nếu cần |

\| Giấy phép | Tên/link giấy phép và điều kiện liên quan |

\| Ghi công | Nội dung ghi công, nơi hiển thị; ghi nhận crop/nén/chỉnh sửa |

\| Xác minh | Ngày kiểm tra và kết luận có căn cứ, không tự nhận quyền |

\| Vị trí dùng | Hero, card tuyến, danh sách chuyến, Liên hệ… |



Giữ hồ sơ các tài nguyên cũ vẫn sử dụng. Không đổi toàn bộ SVG: chỉ thay những hình chính quá đơn điệu; icon, logo, sơ đồ ghế và fallback tiếp tục dùng loại phù hợp.



**## 6. Làm giao diện thực tế, trực quan và thu hút hơn**



Giữ design system **\*\*trắng – xanh ngọc/xanh lá\*\***, font tiếng Việt, thương hiệu véxe. và layout đã có. Không chuyển sang cam FUTA, dark theme hoặc clone thương hiệu khác.



**### Trang chủ**



- Thay minh họa xe ở hero bằng ảnh xe khách/hành trình thật được phép sử dụng, bố cục hình và chữ cân đối.

- Form tìm chuyến vẫn là hành động nổi bật; giữ điểm đi/đến/ngày và GET binding đúng, không đẩy form xuống quá xa vì banner.

- Card gợi ý tuyến có ảnh đúng điểm đến và link truy vấn đúng tuyến; dữ liệu tên tuyến/thời lượng tiếp tục lấy từ backend.

- Nâng khoảng cách, heading, shadow/border, badge và CTA để card rõ hơn, không lấp đầy bằng banner giả.

- Giữ phần hướng dẫn đặt vé, thông tin giữ ghế 5 phút/thanh toán trực tiếp; thêm link Liên hệ thật.

- Không bịa lượt khách, số năm hoạt động, chứng nhận, đánh giá, ưu đãi, giá “chỉ từ” hoặc hotline để tăng độ tin cậy giả.



**### Trang tìm chuyến**



- Thay ảnh xe SVG nhỏ đơn điệu bằng thumbnail phù hợp, có kích thước đủ nhìn nhưng không lấn thông tin giờ/giá/ghế.

- Card thể hiện rõ tuyến, thời gian, xe, số ghế còn tại lần tải, giá mỗi ghế và nút Chọn ghế.

- Tăng phân cấp và tính dễ đọc của thông tin, giữ giá/giờ/số ghế từ dữ liệu thật. Không thêm tiện nghi hoặc chất lượng xe chưa có căn cứ.

- Giữ tham số đổi vé, điều kiện mở bán, bộ lọc, trạng thái không có kết quả và validation.



**### Trang chọn ghế, vé, tài khoản và chính sách**



- Đồng bộ visual với ảnh mới và phần header/footer; chỉ thêm ảnh khi có ích, không chèn ảnh lớn làm khó thao tác ghế hoặc xác nhận.

- Giữ sơ đồ động theo xe/chuyến, chú giải ghế, đồng hồ hold, thông tin hành khách, tổng tiền và trạng thái thu tiền.

- Login/Register/Profile/Policy/chi tiết vé có bố cục gọn, label rõ; không thêm nút hoặc bước không có chức năng.

- Trang Admin/Staff/Driver giữ layout và tác nghiệp đã nâng cấp; chỉ chỉnh nhất quán cần thiết, không dùng ảnh quảng cáo trang trí dashboard hay che bảng.



CSS/JS mới phải tương thích \`ui.css\`, \`admin.css\` và các script hiện tại. Giữ animation nhẹ, hỗ trợ Reduce Motion, focus rõ, bàn phím sử dụng được; không thêm thư viện nặng hoặc carousel autoplay không cần thiết.



**## 7. Thêm trang Liên hệ tham khảo FUTA**



Tham khảo trực tiếp **\*\*https\://futabus.vn/lien-he\*\*** và ảnh tôi gửi. Chỉ học cách chia **\*\*khối thông tin liên hệ\*\*** và **\*\*khối biểu mẫu\*\***, không copy HTML/CSS/logo/thông tin liên hệ doanh nghiệp FUTA.



Yêu cầu giao diện:



- Thêm link **\*\*Liên hệ\*\*** vào navbar desktop/mobile và footer, active state chính xác.

- Có route công khai \`/lien-he\` hoặc route MVC thống nhất được ghi rõ; không đổi các route cũ.

- Tiêu đề/breadcrumb, mô tả hỗ trợ đặt vé và một hình/băng ảnh hành trình gọn nếu phù hợp.

- Desktop bố cục hai cột: thông tin liên hệ bên trái, form bên phải; mobile một cột.

- Thông tin đơn vị thể hiện thương hiệu **\*\*véxe. — hệ thống đồ án\*\***, không giả danh công ty vận tải đã đăng ký kinh doanh.

- Các trường thông tin hỗ trợ như email, điện thoại, địa chỉ lấy từ cấu hình riêng dễ thay. **\*\*Tôi chưa cung cấp hotline/email/địa chỉ vận hành thật, nên để trạng thái “Chưa cấu hình” hoặc hướng dẫn cấu hình rõ, không bịa thông tin, không lấy số của FUTA và không tạo link gọi/gửi email tới địa chỉ giả.\*\***

- Không cần thêm bản đồ/GPS, bộ phận nhiều công ty, fax, chatbot hoặc hotline nổi.



Form tối thiểu:



1. Họ và tên.

2. Email liên hệ.

3. Số điện thoại — có thể tùy chọn vì email đã là kênh phản hồi bắt buộc.

4. Tiêu đề.

5. Nội dung liên hệ.

6. Nút **\*\*Gửi liên hệ\*\*** và lời nhắc không nhập mật khẩu/thông tin thanh toán nhạy cảm.



Field có label, giới hạn độ dài hợp lý, validation tiếng Việt phía client/server, trạng thái focus/loading/error, lỗi từng trường và summary. Không yêu cầu người gửi nhập mã vé nếu chưa có mã. Không hiển thị dropdown đơn vị giống FUTA khi hệ thống chỉ có một thương hiệu.



**## 8. Form Liên hệ phải hoạt động thật, mở rộng tối thiểu**



Được phép bổ sung **\*\*duy nhất chức năng tiếp nhận liên hệ tối thiểu\*\*** phục vụ trang mới. Không biến thành CRM, hệ thống ticket, chat realtime hoặc quy trình chăm sóc khách hàng phức tạp.



- Dùng MVC GET/POST, ViewModel và service phù hợp cấu trúc hiện tại; Guest hoặc người đăng nhập đều gửi được.

- Có anti-forgery, kiểm tra dữ liệu server, giới hạn request phù hợp/chống spam nhẹ, chống submit trùng ở giao diện và xử lý replay tối thiểu nếu cần.

- **\*\*Lưu thông tin vào SQL Server\*\*** trước khi báo thành công; thực hiện Post/Redirect/Get để refresh không gửi lại form.

- Thông báo đúng: “Đã ghi nhận liên hệ trong hệ thống”. Không nói “Email đã gửi” nếu chưa tích hợp và kiểm chứng gửi email.

- Không cần tích hợp SMTP, tài khoản Gmail, email trả phí hoặc SMS. Không tự gửi thông tin cho người thật.

- Lỗi lưu/validation phải trả kết quả thất bại đúng và giữ lại nội dung hợp lệ để sửa; không hiện toast thành công khi database lỗi.

- Nội dung được encode khi hiển thị, không render HTML do người gửi nhập. Không ghi nội dung liên hệ, email/điện thoại đầy đủ vào log không cần thiết.



Nếu chưa có bảng tiếp nhận, được bổ sung **\*\*một bảng nhỏ\*\***, ví dụ \`ContactMessages\` gồm ID, họ tên, email, điện thoại tùy chọn, tiêu đề, nội dung, thời điểm tạo UTC và trạng thái đã đọc tối thiểu. Không thay đổi schema/quan hệ giữ ghế, vé, thanh toán hoặc Identity; không tự thêm workflow phân công/phản hồi hàng loạt.



- Đồng bộ Entity, DbContext, EF Core SQL Server **\*\*migration tăng thêm\*\***, snapshot, ViewModel/service/Controller/View.

- Chỉ tạo migration thêm bảng mới; đọc kỹ SQL sinh ra để đảm bảo không drop/recreate bảng cũ hoặc đổi các cột không liên quan.

- **\*\*Database hiện tại của tôi đã dùng được; giữ nguyên migration history/baseline đang có.\*\*** Không bắt chạy lại Initial migration hoặc script SQL legacy, không xóa lịch sử để vượt lỗi.

- Cung cấp hướng dẫn backup bằng SSMS và áp dụng migration mới phù hợp. Không tự chạy thay đổi lên database riêng của tôi trong môi trường bạn không kiểm soát; kiểm thử trên database test riêng khi có thể.

- Startup không tự reset hoặc seed lại để có bảng mới. Nếu thiếu migration, hướng dẫn rõ thay vì trả thành công giả.



Trong Admin thêm mục nhỏ **\*\*Liên hệ\*\*** để xem danh sách/chi tiết và đánh dấu đã đọc nếu triển khai. Dùng \`\_AdminLayout\` và authorization Admin hiện có, không thêm vai trò. Chỉ Admin được xem nội dung; Guest/Customer/Staff/Driver không được truy cập danh sách/chi tiết bằng URL trực tiếp. Không có trang công khai liệt kê liên hệ hoặc tra nội dung chỉ bằng một ID.



Giữ phần tiếp nhận này gọn, đúng mục đích form hoạt động thật; không ảnh hưởng dashboard doanh thu hoặc các service đặt vé.



**## 9. Yêu cầu kỹ thuật đồ án không được suy yếu**



Tất cả yêu cầu cũ vẫn còn hiệu lực, đặc biệt:



- Tuyến, xe, sơ đồ ghế, ghế chuyến và vé vẫn là những thực thể đúng bản chất.

- Giữ chỗ **\*\*5 phút/300 giây\*\***, \`ExpiresAtUtc\` lưu bền vững, thời gian server là chuẩn; refresh không reset/gia hạn hold.

- Transaction ngắn, constraint/locking chống double booking; không khóa dòng/kết nối suốt 5 phút.

- Expiry worker và kiểm tra hết hạn trong transaction vẫn hoạt động; vé Booked không bị hết hạn theo hold cũ.

- Giữ ownership/token, idempotency, multi-seat all-or-nothing và đổi/hủy nguyên tử theo thiết kế hiện tại.

- SignalR chỉ đồng bộ sau commit, không quyết định quyền ghế; reconnect lấy snapshot, không broadcast thông tin cá nhân/token.

- Giữ các ID/data-attributes/API contract mà \`seats.js\` đang sử dụng; nếu thay giao diện thì phải nối lại đầy đủ.

- Không tạo request giữ/đặt vé từ hover, animation hoặc chỉ vì tải ảnh.

- Giữ toàn bộ nghiệp vụ Customer, Admin, Staff, Driver và thanh toán trực tiếp.



Không thêm thanh toán ngân hàng, ví, voucher/loyalty, marketplace nhiều nhà xe, GPS, quản lý nhiên liệu/bảo dưỡng, AI, forum hoặc ứng dụng mobile. Mọi chỉnh sửa ngoài ảnh/UI/Liên hệ phải là sửa lỗi tối thiểu có lý do và được ghi riêng.



**## 10. Responsive, ảnh và kiểm thử chức năng**



Kiểm tra desktop/mobile ở **\*\*375px, 390px, 768px, 1024px, 1440px\*\***:



- Hero và ảnh card đúng tỷ lệ, không méo/crop mất xe; title không tràn ảnh.

- Form tìm chuyến dễ thấy, navbar có Liên hệ và mobile menu hoạt động.

- Card chuyến không mất giờ/giá/nút Chọn ghế; ảnh không gây tràn ngang.

- Liên hệ hai cột/một cột đúng, validation và textarea/nút Gửi dùng được bằng bàn phím/mobile.

- Sidebar Admin có mục Liên hệ, không mất các mục cũ; layout không che bảng/form.

- Focus nhìn thấy, alt/chú thích đúng, đủ tương phản, hỗ trợ Reduce Motion.

- File local trả về đúng, không hotlink ngoài; thử ảnh fallback và dùng ảnh khi browser offline.



Thử theo môi trường thực tế:



\`\`\`powershell

dotnet --info

dotnet tool restore

dotnet restore BusBooking.sln

dotnet build BusBooking.sln

dotnet ef migrations has-pending-model-changes --project BusBooking

dotnet run --project BusBooking --launch-profile SqlServer

\`\`\`



Nếu cần tạo/áp dụng migration Liên hệ, thực hiện đúng quy trình tăng thêm và hướng dẫn riêng ở mục 8, không thay database connection của tôi hoặc chạy SQL legacy chồng EF.



Kiểm thử tối thiểu:



\| Nhóm | Trường hợp cần chứng minh |

\|---|---|

\| Source/runtime | Restore, build/Razor/DI, khởi động SQL Server và route hợp lệ không phát sinh 500 |

\| Ảnh | File tồn tại trong source/ZIP, đường dẫn đúng, đúng chủ đề, license/ghi công, offline/fallback |

\| Khách | Home → tìm chuyến → chọn ghế → đăng nhập → giữ → xác nhận → xem vé; đổi/hủy giữ nguyên |

\| Realtime | Hai tài khoản chọn cùng ghế, xử lý xung đột, refresh không reset hold, reconnect |

\| Vai trò | Admin vào dashboard sau login; Staff/Driver/Customer đúng quyền; logout vẫn POST an toàn |

\| Liên hệ GET | Guest/người đăng nhập mở được, link header/footer/mobile đúng, cấu hình thiếu không hiện thông tin giả |

\| Liên hệ POST | Dữ liệu hợp lệ tạo bản ghi thật, PRG/refresh không trùng, nội dung sai được từ chối, lỗi DB không báo thành công |

\| Bảo vệ form | Thiếu/sai anti-forgery bị chặn; gửi nhanh liên tiếp bị xử lý hợp lý, payload quá dài bị từ chối |

\| Admin Liên hệ | Xem đúng bản ghi vừa gửi, encode chuỗi HTML/XSS, cập nhật đã đọc đúng; các role khác bị chặn |

\| UI/browser | Các viewport, menu, focus, Reduce Motion, console/network, không tải asset lỗi |



Tái sử dụng bộ kiểm thử SQL/concurrency hiện có khi môi trường cho phép; không bỏ yêu cầu test 20/50/100 request hoặc đổi số liệu đo cũ thành kết quả mới. Không cần sửa thuật toán giữ chỗ cho nhiệm vụ ảnh/Liên hệ.



**\*\*Chỉ ghi PASS cho test đã chạy và có kết quả.\*\*** SQL Server đã kết nối trên máy tôi không chứng minh bạn đã test trong môi trường của bạn. Nếu chưa chạy được .NET/SQL Server/browser thì ghi **\*\*CHƯA KIỂM CHỨNG\*\***, nêu rõ phần đã/ chưa kiểm và không dùng ảnh mockup thay screenshot thật.



**## 11. Tài liệu phải cập nhật theo source thực tế**



- \`README.md\`: chạy .NET 9, giữ SQL Server hiện có, quy trình cập nhật và route Liên hệ.

- \`ASSET_SOURCES.md\`: từng ảnh local, trang gốc, tác giả, giấy phép, ghi công và vị trí dùng.

- \`IMAGE_INTEGRATION_REPORT.md\`: ảnh trước/sau, kích thước/tỷ lệ/dung lượng, mapping/fallback, phần chưa có ảnh phù hợp.

- \`CONTACT_PAGE_GUIDE.md\`: field/form, nơi lưu, quyền xem, cấu hình thông tin hỗ trợ, trạng thái chưa tích hợp email.

- \`DATABASE_CHANGELOG.md\`: migration thêm bảng Liên hệ, script đã rà, backup và hướng dẫn cập nhật không mất dữ liệu; không tuyên bố đã nâng DB của tôi khi chưa thực hiện.

- \`UI_CHANGELOG.md\`, \`FILE_CHANGES.md\`: file CSS/JS/Razor/backend tối thiểu được sửa, không ghi chung chung.

- \`DESIGN_REFERENCE_REVIEW\.md\`: FUTA Liên hệ, ngày khảo sát, điểm học hỏi, phần không sao chép.

- \`TEST_RESULTS.md\`: bảng PASS/FAIL/CHƯA KIỂM CHỨNG, lệnh, môi trường, bằng chứng, giới hạn.

- Giữ các tài liệu đề tài/ERD/permission/state machine/hold/realtime/concurrency; cập nhật phần Liên hệ khi có liên quan, không xóa yêu cầu gốc.

- Cập nhật source manifest/checksum nếu project đã có.



**## 12. Bàn giao ZIP mới và phản hồi cuối**



Xuất một ZIP source hoàn chỉnh:



**\*\*\`QuanLyXeKhach_DatVe_GiuCho_REALTIME_IMAGES_CONTACT_REVIEWED.zip\`\*\***



Chỉ đổi hậu tố thành **\*\*\`VERIFIED\`\*\*** khi thực sự restore/build/run với SQL Server và kiểm thử thành công luồng đặt vé cốt lõi, phân quyền, ảnh và form Liên hệ lưu dữ liệu thật. Ghi riêng những mục chưa thử; không xem static check là chạy thành công.



ZIP phải chứa solution/project hiện tại, MVC/Identity/EF SQL Server/migrations, Controllers/Services, Razor, worker/Hub, CSS/JS/animation, **\*\*toàn bộ ảnh local có quyền dùng\*\***, Liên hệ hoạt động, test và tài liệu.



Không đóng gói \`bin/obj/.vs/node_modules\`, secrets, database/backup riêng, dữ liệu cá nhân hoặc nội dung liên hệ thật. Không đưa mật khẩu/connection string nhạy cảm vào ZIP; cấu hình qua user-secrets/environment như hệ thống đang có.



Phản hồi cuối gồm:



1. Link tải ZIP source mới.

2. Danh sách ảnh đã tải thật: tên local, nội dung, nguồn/quyền và nơi dùng.

3. Trang/khu vực đã nâng cấp; route và chức năng Liên hệ.

4. Thay đổi database tối thiểu, cách backup và cập nhật bản đang có SQL Server.

5. Kết quả build/run/HTTP/browser, bảng PASS/FAIL/CHƯA KIỂM CHỨNG.

6. Screenshot thật Home desktop/mobile, tìm chuyến, Liên hệ và Admin tiếp nhận liên hệ nếu chạy được.

7. Hướng dẫn giải nén, mở \`BusBooking.sln\`, giữ cấu hình hiện tại và chạy lại trên Visual Studio.



**\*\*Ưu tiên giữ website đang chạy ổn định. Chỉ nâng cấp ảnh, phần trình bày và Liên hệ theo yêu cầu này. Giữ nguyên tên đề tài, chuyên ngành, .NET 9, SQL Server, nghiệp vụ và toàn bộ yêu cầu còn lại. Không gửi ZIP mà ảnh vẫn là link ngoài hoặc form Liên hệ chỉ có giao diện.\*\***


