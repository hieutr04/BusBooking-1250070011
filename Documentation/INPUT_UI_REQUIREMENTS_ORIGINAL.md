**# PROMPT NÂNG CẤP TOÀN BỘ GIAO DIỆN BUSBOOKING — TRẮNG & XANH, ĐÚNG NGHIỆP VỤ THƯƠNG MẠI ĐIỆN TỬ**



**## 1. Nhiệm vụ và tên đề tài không được thay đổi**



Tôi đang thực hiện đồ án chuyên ngành **\*\*Thương mại điện tử\*\***, tên đề tài chính thức:



**\*\*XÂY DỰNG HỆ THỐNG QUẢN LÝ XE KHÁCH VÀ ĐẶT VÉ TRỰC TUYẾN TÍCH HỢP CƠ CHẾ GIỮ CHỖ THEO THỜI GIAN THỰC\*\***



**\*\*Giữ nguyên tên đề tài, nghiệp vụ, bốn vai trò, mô hình dữ liệu và toàn bộ yêu cầu đã được xác lập. Đây là nhiệm vụ nâng cấp giao diện và sửa chuyển hướng sau đăng nhập, không phải xây lại hệ thống hoặc thay đổi phạm vi đồ án.\*\***



Đầu vào tôi đính kèm:



1. Source hiện tại: \`QuanLyXeKhach_DatVe_GiuCho_REALTIME_REVIEWED(1).zip\` hoặc bản mới nhất tôi gửi.

2. Ảnh giao diện \`/Admin\` hiện tại để chỉ ra phần quản trị còn sơ sài.

3. Đề cương Word và ảnh nhận xét giảng viên, nếu được đính kèm lại trong phiên làm việc.



**\*\*Hãy trực tiếp sửa source trong ZIP hiện tại, kiểm tra rồi bàn giao ZIP source mới. Không chỉ gửi kế hoạch, prompt tiếp theo, mockup, trang HTML tĩnh hoặc đoạn code rời.\*\***



Không chỉnh sửa tài liệu yêu cầu gốc. Nếu Word/ảnh nhận xét cũ chưa được đính kèm trong phiên này, nói rõ chưa đọc được; sử dụng yêu cầu đã lưu trong \`Documentation/INPUT_REQUIREMENTS_ORIGINAL.md\`, tài liệu nghiệp vụ và source hiện tại để giữ phạm vi. Không giả vờ đã truy cập được tệp trong cuộc trò chuyện khác.



**## 2. Hiện trạng source và audit bắt buộc**



ZIP được cung cấp hiện có thư mục gốc \`BusBooking\`, solution \`BusBooking.sln\`, project web \`BusBooking/BusBooking.csproj\`, target \`net9.0\`, Identity/EF Core SQL Server 9.0.9. Tên kỹ thuật là BusBooking, thương hiệu giao diện hiện tại là **\*\*véxe.\*\***; giữ thương hiệu và tên đề tài đang có.



Các thành phần đã có cần kiểm tra trước khi sửa:



- Controllers: \`Account\`, \`Admin\`, \`Bookings\`, \`Driver\`, \`Home\`, \`SeatsApi\`, \`Staff\`, \`Trips\`.

- \`Data/AppDbContext.cs\`, SQL Server migrations/snapshot, \`SeedData.cs\`.

- \`BookingService\`, \`CatalogService\`, \`ReportService\`, \`RealtimePublisher\`, \`RealtimeMetrics\` và dịch vụ hạ tầng.

- \`BackgroundServices/HoldExpiryWorker.cs\`, \`Hubs/SeatsHub.cs\`.

- 31 Razor files hiện có, gồm cả layout/partial; phải kiểm kê đầy đủ, không chỉ nâng cấp trang chủ.

- \`wwwroot/css/site.css\`, \`wwwroot/js/site.js\`, \`seats.js\`, \`dashboard.js\`.

- Bootstrap, SignalR client và Chart.js local; các ảnh SVG local và hồ sơ nguồn tài sản.

- Bộ kiểm thử \`Tests/BusBooking.SqlTests\`, \`Tests/browser.mjs\`, script kiểm tra source và các tài liệu đối chiếu yêu cầu.



Lập bảng trang/action → vai trò → View → chức năng → thay đổi UI → kiểm thử tương ứng. Ghi nhận baseline trước chỉnh sửa; không coi README cũ hoặc ảnh người dùng là bằng chứng source này đã build/run trong môi trường của bạn.



**\*\*Lỗi chuyển hướng đã thấy trong bản đầu vào:\*\*** \`AccountController.Login\` trả về \`Home/Index\` khi đăng nhập thành công nhưng không có \`ReturnUrl\` local. \`Views/Admin/Index.cshtml\` đang sử dụng layout khách hàng và partial \`\_AdminNav\`; vì vậy trang quản trị chưa có cấu trúc dashboard riêng. Kiểm tra lại code thực tế trước khi sửa vì bản đầu vào có thể đã được cập nhật.



**## 3. Công nghệ và dữ liệu phải giữ nguyên**



- ASP.NET Core MVC **\*\*.NET 9\*\***, C#, Razor Views, HTML/CSS/JavaScript.

- \`TargetFramework=net9.0\`; SDK 9.0.305 và cấu hình \`global.json\` tương thích hiện có. Không chuyển .NET 8 hoặc .NET 10.

- EF Core SQL Server, ASP.NET Core Identity, Cookie Authentication, RBAC.

- SQL Server là kho dữ liệu thật; giữ connection string bằng cấu hình/user-secrets, migrations và dữ liệu hiện tại.

- Bootstrap 5, Chart.js, SignalR, async/await và thư viện local đang dùng.

- Giữ tên solution/project, route, Controller/action, service contract và API contract trừ điều chỉnh tối thiểu có lý do rõ ràng.



Không thêm SQLite Demo, EF InMemory, JSON backend, fallback giả, Preview chỉ đọc hoặc cơ chế tự đổi database. Không tự xóa/reset database, chạy seed trùng, đổi mật khẩu/tài khoản người dùng, drop bảng hoặc xóa migration history.



Nâng UI không mặc nhiên cần migration. Nếu không thay entity/schema thì **\*\*không tạo migration\*\***. Nếu phát hiện lỗi cũ gây chặn chức năng, sửa tối thiểu, giải thích riêng và kiểm tra hồi quy; không tái cấu trúc backend diện rộng.



**## 4. Tham khảo thiết kế công khai, không clone**



Sử dụng công cụ web search/trình duyệt được hỗ trợ để khảo sát nguồn thực tế:



\| Nguồn | Nội dung tham khảo | Cách áp dụng vào BusBooking |

\|---|---|---|

\| https\://futabus.vn/ | Cách tổ chức website vận tải, điều hướng và luồng tìm/đặt/tra cứu vé | Biểu mẫu tìm chuyến nổi bật, thông tin hành trình rõ, thao tác đặt vé có thứ tự |

\| https\://vexere.com/ | Tham khảo thêm cách trình bày kết quả chuyến và thông tin đặt vé, nếu truy cập được | Học cách phân cấp thông tin, không đưa marketplace nhiều nhà xe vào đồ án |

\| https\://preview\.tabler.io/ và https\://tabler.io/ | Dashboard, sidebar, biểu mẫu, bảng và trạng thái giao diện | Quản trị sáng màu, cấu trúc tác nghiệp rõ ràng |

\| https\://demo-basic.adminkit.io/ và https\://adminkit.io/ | Bố cục dashboard và tổ chức bảng/biểu đồ | KPI ngắn gọn, biểu đồ hữu ích, bảng dữ liệu dễ đọc |



Có thể tìm thêm bằng Google với từ khóa “bus booking website UI”, “light admin dashboard Bootstrap 5”, “transport booking admin dashboard”. Google chỉ dùng tìm nguồn; mở trang gốc để đối chiếu. Ghi rõ trang nào không truy cập được, không mô tả những phần chưa xem như đã khảo sát.



Chỉ học nguyên tắc UX và bố cục; thiết kế nhận diện riêng của **\*\*véxe.\*\*** bằng trắng và xanh. Không sao chép thương hiệu FUTA/Vexere, logo, banner, số điện thoại, nội dung, ảnh, mã HTML/CSS hoặc dashboard demo. Không lấy màu cam của FUTA làm màu chính.



Không đưa toàn bộ template nặng vào project chỉ để có sidebar. Ưu tiên tận dụng Bootstrap và CSS hiện có. Nếu dùng component/icon/template bên ngoài, kiểm giấy phép của đúng gói được dùng, đóng gói local và giữ thông báo giấy phép. Không lấy template trả phí từ nguồn không hợp lệ.



Tạo \`DESIGN_REFERENCE_REVIEW\.md\`: URL, ngày khảo sát, trang/phần đã xem, ý tưởng áp dụng, phần không áp dụng do ngoài phạm vi, giới hạn truy cập.



**## 5. Design system trắng và xanh**



Định hướng: **\*\*website đặt vé trực tuyến và điều hành xe khách hiện đại, trực quan, dễ tin cậy, đúng nghiệp vụ thương mại điện tử\*\***.



Giữ nền trắng, xanh ngọc/xanh lá đang có làm chủ đạo. Có thể tinh chỉnh đồng bộ theo bộ màu gợi ý:



\`\`\`css

:root {

  --brand-primary: #087f73;

  --brand-hover: #06675e;

  --brand-soft: #e9f6f3;

  --page-bg: #f5f8f7;

  --surface: #ffffff;

  --text-main: #15343b;

  --text-muted: #52666d;

  --border-color: #dce7e4;

  --danger: #b42318;

  --warning: #8a5700;

  --radius-card: 16px;

  --radius-control: 10px;

}

\`\`\`



- Quản lý màu, spacing, radius, shadow bằng CSS variables; kiểm tương phản thực tế.

- Font tiếng Việt đầy đủ, ưu tiên system font hoặc font có giấy phép và file local.

- Heading có phân cấp, nội dung đọc dễ; không dùng tiêu đề quá lớn gây lãng phí vùng tác nghiệp như ảnh Admin hiện tại.

- Container hợp lý; card, form, bảng và khoảng cách thống nhất giữa mọi trang.

- Trạng thái thành công/cảnh báo/lỗi dùng cả chữ hoặc icon, không chỉ màu.

- Có hover/focus/pressed/disabled/loading; thông báo phản ánh kết quả server thật.

- Nội dung bằng tiếng Việt; dùng \`vi-VN\` cho tiền, hiển thị giờ Việt Nam, lưu và xử lý thời gian theo quy tắc UTC của hệ thống hiện tại.



Giữ thông tin đây là đồ án/dữ liệu thử nghiệm đúng vị trí, không biến thành quảng cáo doanh nghiệp vận tải thật. Không tạo đánh giá, lượt khách, chứng nhận, khuyến mại hoặc hotline giả để trang trông giống thương mại điện tử.



**## 6. Nâng cấp đầy đủ các trang khách hàng**



**### 6.1. Header, footer và trang chủ — \`Home/Index\`**



- Header logo véxe., tìm chuyến, chính sách đổi/trả, vé của tôi, tài khoản theo đúng quyền.

- Active navigation rõ, menu mobile dễ dùng, dropdown bàn phím sử dụng được.

- Hero nền sáng với hình phương tiện/hành trình phù hợp, thông điệp ngắn và **\*\*form tìm chuyến là thao tác chính\*\***.

- Form điểm đi, điểm đến, ngày đi giữ đúng tên field, GET action và quy tắc lọc hiện tại. Nút đổi điểm đi/đến chỉ thêm nếu thực sự hoạt động với dữ liệu hợp lệ.

- Tuyến gợi ý lấy từ dữ liệu thật đang có; link mở đúng kết quả tìm kiếm. Không tạo giá “từ…” nếu chưa có truy vấn chứng minh.

- Có hướng dẫn đặt vé ngắn, giải thích giữ chỗ 5 phút, thanh toán trực tiếp và đường dẫn chính sách đang có.

- Footer rõ tên đề tài, thông tin học tập và các link đang hoạt động; không thêm module tin tức/quảng cáo không thuộc đề tài.



**### 6.2. Tìm chuyến — \`Trips/Index\`**



- Thanh tìm kiếm gọn, giữ các lựa chọn sau khi tìm và sau validation.

- Card chuyến có tuyến, ngày/giờ đi và đến dự kiến, xe/loại xe, ghế khả dụng, giá/ghế, nút xem/chọn ghế.

- Dùng dữ liệu sẵn có; không bịa tiện ích xe, xếp hạng, điểm đón trả hoặc thời lượng chưa được cung cấp.

- Mọi bộ lọc/sắp xếp đưa vào UI phải có logic thật; không dựng control giả. Không đổi điều kiện chỉ hiển thị chuyến còn được phép bán.

- Có trạng thái không tìm thấy chuyến, dữ liệu lỗi và hướng dẫn sửa tiêu chí tìm.

- Bảo toàn tham số \`exchangeBookingId\` khi người dùng đang đổi vé.



**### 6.3. Chọn ghế và xác nhận — \`Trips/Details\`**



- Trình bày thông tin hành trình phía trên, sơ đồ ghế rõ ở vùng chính, tóm tắt đặt vé ở bên cạnh; mobile xếp theo thứ tự dễ thao tác.

- Tầng/hàng/cột và lối đi phải theo sơ đồ dữ liệu, không vẽ một sơ đồ tĩnh cho mọi xe.

- Phân biệt ghế trống, đang chọn, đang giữ bởi người khác, lượt giữ của tôi, đã đặt; có chú giải chữ và trạng thái truy cập được.

- Hiển thị kết nối SignalR, đồng bộ lại, thông báo xung đột và thời gian giữ thực tế từ server.

- Tóm tắt đúng ghế, số lượng, giá và tổng; biểu mẫu thông tin người liên hệ/hành khách dễ đọc.

- Giữ các chức năng giữ 1–6 ghế toàn bộ hoặc không giữ, lựa chọn lượt giữ, nhả giữ, khôi phục hold, xác nhận và đổi vé hiện có.

- Không che nút bởi thanh sticky, bàn phím mobile hoặc thông báo lỗi. Không thêm bước chọn phương thức thanh toán ngân hàng.



**### 6.4. Vé của tôi và chi tiết — \`Bookings/Index\`, \`Bookings/Details\`**



- Danh sách vé rõ mã, tuyến, ngày đi, ghế, trạng thái vé và trạng thái thu tiền riêng biệt.

- Chi tiết gồm hành trình, hành khách, giá chốt, khoản đã thu/đã hoàn/chờ hoàn theo dữ liệu thật.

- Các nút yêu cầu đổi/hủy, xử lý của nhân viên và liên kết đổi chuyến phải giữ nguyên điều kiện/quyền.

- Không gọi vé “đã thanh toán” chỉ vì đặt thành công. Không tạo QR/check-in hoặc hóa đơn điện tử mới nếu không có nghiệp vụ đó.

- Giữ kiểm tra chủ sở hữu; đổi ID trên URL không được xem hoặc thao tác vé người khác.



**### 6.5. Tài khoản, hồ sơ, chính sách và trang lỗi**



- Nâng UI \`Account/Login\`, \`Register\`, \`Profile\`; label rõ, validation summary + lỗi từng field, autocomplete phù hợp.

- Có thể thêm nút hiện/ẩn mật khẩu với nhãn truy cập được; không thay đổi cách xác thực.

- Không có nút quên mật khẩu, đăng nhập Google hoặc thông báo email nếu chưa có chức năng thật.

- Đăng ký công khai vẫn chỉ tạo Customer; không cho tự chọn Admin/Staff/Driver.

- \`Home/Policy\` dễ đọc, phân đoạn rõ; giữ nguyên quy định nghiệp vụ đang công bố.

- \`Shared/Problem\` phân biệt 400/403/404/409/500/503 theo kết quả thật, có đường quay lại phù hợp; không trả HTTP 200 giả.



**## 7. Sửa đăng nhập và điều hướng theo vai trò — bắt buộc**



Sửa ở **\*\*server\*\***, trong luồng Identity/MVC hiện có; không dùng JavaScript đổi URL sau đăng nhập để che lỗi backend.



Hành vi mong muốn khi đăng nhập hợp lệ:



\| Vai trò | Trang mặc định | Quy tắc điều hướng |

\|---|---|---|

\| Admin | \`/Admin\` — \`Admin/Index\` | Đăng nhập xong vào khu quản trị; không bị \`returnUrl=/\` hoặc URL khách hàng kéo về trang chủ. Chỉ giữ returnUrl nội bộ thuộc khu tác nghiệp được Admin phép truy cập, khi phù hợp |

\| Staff | \`/Staff\` — \`Staff/Index\` | Vào phòng vé; có thể phục hồi luồng nội bộ hợp lệ đang thực hiện |

\| Driver | \`/Driver\` — \`Driver/Index\` | Vào lịch được phân công; không vào chức năng ngoài quyền |

\| Customer | ReturnUrl nội bộ phù hợp hoặc trang chủ/tìm chuyến | Đặc biệt phục hồi chuyến đang xem để tiếp tục đặt vé sau đăng nhập |



- Nếu tài khoản có nhiều vai trò, ưu tiên nhất quán **\*\*Admin → Staff → Driver → Customer\*\***, không thay dữ liệu role để đạt mục tiêu này.

- \`returnUrl\` phải được kiểm tra local và giới hạn theo quyền/đích hợp lệ. Loại URL ngoài, URL dạng protocol-relative, đích Login/Register/Logout hoặc vòng lặp. Không chỉ dựa vào chuỗi URL từ client để cấp quyền.

- Nếu người đã đăng nhập mở lại trang Login, điều hướng về workspace phù hợp; không tạo vòng lặp và không tự biến truy cập Home thông thường thành redirect toàn cục.

- Admin vẫn có thể chủ động bấm “Xem website” để vào trang khách hàng.

- Giữ \`PasswordSignInAsync\`, trạng thái \`IsActive\`, lockout, validation, anti-forgery, SignOut và authorization hiện có.

- Tài khoản không đúng/sai mật khẩu/bị khóa/không hoạt động vẫn thất bại đúng cách. Không đăng nhập giả hoặc bỏ qua Identity.

- Không cho Staff/Customer/Driver vào Admin bằng URL trực tiếp. Ẩn menu không thay thế kiểm tra server.

- Logout tiếp tục POST có anti-forgery; sau đăng xuất, truy cập workspace lại phải bị chặn.



Tạo testcase riêng cho từng vai trò, nhiều vai trò, returnUrl trống/nội bộ/ngoài quyền/ngoài website và truy cập trực tiếp trang bị cấm.



**## 8. Redesign toàn bộ quản trị — ưu tiên cao**



Trang \`/Admin\` phải là **\*\*workspace quản lý thực sự\*\***, không chỉ trang khách hàng có thêm hàng menu ngang.



**### 8.1. Layout quản trị riêng**



- Có thể tạo \`\_AdminLayout.cshtml\` và partial sidebar/header, áp dụng rõ cho các View Admin bằng \`\_ViewStart.cshtml\` phù hợp hoặc khai báo layout.

- Sidebar trái: Tổng quan, Tuyến xe, Đội xe, Sơ đồ ghế, Chuyến & phân công, Tài khoản, Vé/thu tiền, Nhật ký.

- Các link phải trỏ tới action hiện có; “Vé/thu tiền” tiếp tục dùng chức năng \`Staff\` được Admin cấp quyền, không tạo một module booking thứ hai.

- Header gọn: tên trang, breadcrumb, tài khoản/vai trò, Xem website, Đăng xuất.

- Active item chính xác; mobile sidebar dạng offcanvas có nút đóng, Escape, focus quản lý đúng.

- Nền trắng/xanh nhạt, content rộng hợp lý, bảng đủ chỗ; không dùng font/khoảng trắng/tiêu đề quá lớn của landing page cho tác nghiệp.

- Layout mới phải giữ render validation, TempData, anti-forgery, CSS/JS và \`@RenderSectionAsync("Scripts", required: false)\` để Chart.js và script từng trang còn chạy.

- Không cài Bootstrap hai lần hoặc làm CSS quản trị ghi đè hỏng trang khách.



**### 8.2. Dashboard \`Admin/Index\`**



- Tiêu đề gọn, mô tả ngắn, bộ lọc từ ngày/đến ngày Việt Nam, nút áp dụng và validation ngày.

- KPI hiện có: chuyến trong kỳ theo ngày khởi hành; vé theo ngày tạo; tiền thực thu; thu ròng = thu − hoàn. Trình bày bằng icon/card rõ và số liệu thật từ \`ReportService\`.

- Biểu đồ thu/hoàn theo tuyến dùng Chart.js hiện có; chú giải, đơn vị và vùng hiển thị hợp lý.

- Bảng doanh thu theo tuyến, đối soát vé đã thu/chưa thu, đã hoàn/chờ hoàn dễ đọc.

- **\*\*Giữ nguyên cách tính và mốc thời gian của từng KPI.\*\*** Hold không phải vé bán; vé đặt chưa thu không phải tiền đã thu; số chờ hoàn toàn hệ thống không được đổi thành số theo kỳ mà không sửa nghiệp vụ có căn cứ.

- Không thêm visitor, phần trăm tăng trưởng, lợi nhuận, vé “thành công hôm nay” hoặc biểu đồ giả từ template. Dữ liệu bằng 0 có empty state đúng, không dùng số mẫu để lấp chỗ trống.

- Có lối tắt tới các tác vụ đang tồn tại, không tạo nút xuất Excel/PDF hoặc tính năng chưa có backend.



**### 8.3. Các trang quản lý còn lại**



Nâng cấp đủ \`Admin/Routes\`, \`RouteEdit\`, \`Vehicles\`, \`VehicleEdit\`, \`Layouts\`, \`LayoutEdit\`, \`Trips\`, \`TripEdit\`, \`Users\`, \`Audit\`.



- Toolbar, tiêu đề, hành động thêm/sửa, bảng, badge trạng thái, form thống nhất.

- Dữ liệu tuyến, xe, sơ đồ, chuyến, phân công và tài khoản dùng đúng ViewModel/model binding cũ.

- Biểu mẫu có nhóm thông tin, label, trợ giúp và lỗi tại chỗ. Không bỏ trường quan trọng vì bố cục mới.

- Sơ đồ ghế có preview từ cấu hình thật nếu triển khai được mà không thay quy tắc lưu; không giả vờ preview là dữ liệu đã lưu.

- Mở/đóng bán, hủy chuyến, đổi vai trò/khóa tài khoản và thao tác nhạy cảm phải giữ đúng service, điều kiện, xác nhận và anti-forgery.

- Không thêm chức năng xóa vật lý dữ liệu lịch sử chỉ vì template có nút Delete.

- Có thể bổ sung tìm kiếm/lọc/phân trang tối thiểu cho bảng dài nếu cần UX; phải làm thật phía server, giữ tham số, quyền và kiểm thử, không chỉ lọc 200 dòng đã bị \`.Take(...)\` rồi gọi đó là tìm toàn bộ hệ thống. Nếu chưa làm thì không hiển thị control giả.

- Nhật ký giữ nguồn dữ liệu thật; không thêm dữ liệu tài khoản/thông tin cá nhân từ demo template.



**## 9. Nâng cấp phòng vé và tài xế đầy đủ**



**\*\*Staff:\*\*** \`Staff/Index\`, \`Requests\`, \`Trips\`, \`Shared/Passengers\`, các phần thao tác trên chi tiết vé.



- Workspace tác nghiệp đồng bộ với Admin nhưng menu đúng quyền.

- Tra mã vé, bán hỗ trợ, liên kết Customer, xác nhận thu tiền trực tiếp, yêu cầu đổi/hủy, từ chối có lý do, ghi nhận hoàn, danh sách hành khách phải dễ dùng và giữ workflow hiện tại.

- Giữ cùng \`BookingService\` với kênh khách hàng; không có đường bán bỏ qua hold hoặc chống trùng.

- Nếu Admin mở trang Staff, giữ trải nghiệm điều hướng Admin phù hợp; Staff không thấy chức năng chỉ dành Admin.



**\*\*Driver:\*\*** \`Driver/Index\`, action \`Driver/Details\` đang dùng chung \`Shared/Passengers\`.



- Lịch được phân công trình bày gọn: chuyến, tuyến, giờ, xe, trạng thái, nút xem hành khách.

- Chuyển trạng thái chuẩn bị/đang chạy/hoàn thành đúng service và điều kiện, có loading/xác nhận hợp lý.

- Không hiển thị doanh thu quản trị, thông tin ngoài quyền hoặc danh sách chuyến người khác.



Phải rà cả View dùng chung, không chỉ các thư mục theo tên Controller. Nếu chọn layout theo role cho View dùng chung, thực hiện rõ ràng, không để Staff/Driver bị lọt menu Admin.



**## 10. Bảo toàn trọng tâm giữ ghế thời gian thực**



Phần này là yêu cầu bất biến; nâng UI không được làm suy yếu kỹ thuật đồ án:



1. SQL Server quyết định quyền giữ/đặt ghế; không dùng DOM, localStorage, cache trình duyệt hoặc SignalR làm nguồn quyết định.

2. Giữ chỗ **\*\*300 giây\*\***, thời hạn lưu bền vững bằng \`ExpiresAtUtc\`/cơ chế hiện có; thời gian server là chuẩn. Refresh/animation không gia hạn hoặc reset hold.

3. Chỉ dùng transaction ngắn để kiểm tra/cập nhật; giữ nguyên constraint/locking cần thiết. Không giữ khóa dòng, transaction hoặc connection mở suốt 5 phút.

4. Trạng thái trống → giữ → đặt, hết hạn/nhả → trống; hủy/đổi giải phóng nguyên tử và giữ lịch sử.

5. Background worker và kiểm tra hết hạn trong giao dịch vẫn hoạt động. Vé Booked không bị expiry của hold cũ trả về trống.

6. Giữ token/quyền sở hữu, idempotency, chống double booking, all-or-nothing với nhiều ghế.

7. SignalR chỉ phát trạng thái sau commit; không phát hold token/thông tin hành khách ra nhóm công khai.

8. Reconnect lấy snapshot, xử lý bản tin cũ/version theo cơ chế hiện có. Mất WebSocket không được làm sai dữ liệu hoặc báo đặt thành công giả.

9. Giữ API contract và các ID/data-attributes mà \`seats.js\` dùng. Đặc biệt rà \`seat-app\`, \`seat-map\`, \`hold-button\`, \`checkout-form\`, \`checkout-panel\`, \`hold-list\`, \`countdown\`, \`confirm-button\`, \`release-button\`, \`connection-status\`, \`seat-message\`, field hành khách và tham số đổi vé. Nếu đổi thì sửa đồng bộ và kiểm thử toàn luồng.

10. Thao tác đang gửi chỉ khóa phần liên quan; thất bại phải mở lại, giữ dữ liệu nhập phù hợp và hiện lỗi đúng HTTP. Không đăng ký handler hoặc submit hai lần.



Không bổ sung cổng thanh toán thật, POS, ví/voucher, GPS, AI/chatbot, quản lý bảo dưỡng/nhiên liệu, marketplace, forum hoặc ứng dụng di động. “Chuẩn thương mại điện tử” ở đây là UX giao dịch rõ ràng và dữ liệu chính xác, không phải mở rộng đề tài.



**## 11. CSS, JavaScript, ảnh và animation**



- Audit CSS/JS trước sửa; tách \`admin.css\`, \`admin.js\` hoặc module nhỏ chỉ khi có ích, giữ thứ tự và phạm vi load.

- Ưu tiên JavaScript thuần, Bootstrap đang có; không chuyển React/Vue hoặc thêm thư viện animation nặng.

- Hover/fade/transition nhẹ; dùng transform/opacity, IntersectionObserver nếu cần reveal dưới màn hình đầu.

- Nội dung vẫn hiện khi JS bị tắt; luồng Razor thông thường không bị animation khóa tương tác. Sơ đồ realtime cần JS thì hiển thị thông báo rõ khi không chạy được.

- Hỗ trợ \`prefers-reduced-motion: reduce\`, focus nhìn thấy, keyboard navigation, label, semantic HTML, alt, aria-live phù hợp; không đọc countdown mỗi giây qua screen reader.

- Loading/toast từ kết quả thật; không hiện thành công trước commit. Không tạo skeleton giả cho phần Razor đã render xong.

- Không dùng animation/transition tất cả thuộc tính, autoplay banner gây phân tâm hoặc scroll handler nặng.

- Ảnh đúng chủ đề xe/hành trình, không kéo giãn, có aspect-ratio/object-fit/object-position phù hợp; ảnh dưới fold lazy-load.

- Giữ ảnh đã có nguồn; nếu thêm ảnh/font/icon chỉ dùng tài sản có quyền xác minh, file nằm trong \`wwwroot\`, ghi \`ASSET_SOURCES.md\`. Không lấy ảnh trên FUTA/Vexere/Google Images khi chưa có quyền, không hotlink ảnh tạm hoặc xóa watermark.

- Không cần thay ảnh chỉ để tăng số lượng. SVG hợp lệ vẫn dùng cho icon/minh họa; ưu tiên hình ảnh chất lượng cho hero khi có nguồn phù hợp.



**## 12. Responsive và trải nghiệm xuyên suốt**



Kiểm tra ít nhất **\*\*375px, 390px, 768px, 1024px và 1440px\*\*** cho cả trang khách, Admin, Staff và Driver:



- Không tràn ngang toàn trang; bảng dài có vùng cuộn riêng, header/cột hành động không che thao tác.

- Không mất menu, nút submit, validation hoặc thanh xác nhận ở mobile.

- Sơ đồ ghế giữ đúng hình học và mã ghế; nếu cần cuộn nội bộ phải rõ ràng, không ép đến mức không bấm được.

- Form input/date/select dùng được, label không cắt, tiền/mã vé dài không phá card.

- Sidebar/mobile menu đóng mở đúng và không kẹt focus; zoom 200% vẫn thao tác được.

- Không dùng chỉ hover để lộ nút quan trọng. Mục tiêu bấm đủ lớn cho màn hình cảm ứng.

- Khoảng trống, chiều cao card và chart cân đối; không sao chép nguyên desktop xuống mobile.



**## 13. Kiểm thử thật, không chỉ kiểm tra cú pháp**



Thực hiện các lệnh phù hợp với máy thực tế, ghi log:



\`\`\`powershell

dotnet --info

dotnet tool restore

dotnet restore BusBooking.sln

dotnet build BusBooking.sln

dotnet ef migrations has-pending-model-changes --project BusBooking

dotnet run --project BusBooking --launch-profile SqlServer

\`\`\`



Dùng SQL Server được cấu hình hợp lệ; không ghi đè user-secrets hoặc tự suy đoán instance/mật khẩu. Nếu không có SQL Server/runtime thì nêu rõ phần bị chặn và **\*\*CHƯA KIỂM CHỨNG\*\***, không dùng database giả để báo PASS. Không chạy migration/seed vào database riêng của tôi khi chưa có phạm vi rõ; dùng DB test riêng nếu kiểm thử ghi dữ liệu.



Các kiểm tra bắt buộc:



\| Nhóm | Kịch bản tối thiểu |

\|---|---|

\| Source | Restore/build, Razor, DI, CSS/JS load, ảnh local, route, lỗi console |

\| Khách | Home → tìm chuyến → chi tiết → đăng nhập phục hồi chuyến → giữ → xác nhận → vé của tôi/chi tiết |

\| Đăng nhập | Admin không returnUrl vào \`/Admin\`; returnUrl Home không kéo Admin về Home; Staff/Driver/Customer về đúng nơi; redirect ngoài bị từ chối; logout |

\| Quyền | Guest và ba vai trò không phải Admin bị chặn khỏi Admin; không truy cập vé/chuyến phân công người khác; POST nhạy cảm và CSRF vẫn được bảo vệ |

\| Admin | Dashboard filter/KPI/chart/bảng, toàn bộ trang quản lý và form GET/POST, menu/active state |

\| Staff | Tìm vé, bán hỗ trợ, thu/hoàn trực tiếp, yêu cầu đổi/hủy, hành khách |

\| Driver | Chỉ chuyến được giao, danh sách khách, chuyển trạng thái đúng quy tắc |

\| Realtime | Hai tài khoản tranh cùng ghế; đồng bộ sau commit; nhả/hết hạn; refresh không reset; reconnect và server từ chối hold cũ |

\| Đổi/hủy | Giữ tham số đổi vé, service nguyên tử, lỗi không mất vé cũ, lịch sử và khoản thu/hoàn không bị thay đổi |

\| UX | Các viewport, menu mobile, bàn phím/focus, Reduce Motion, loading/failure và validation |

\| HTTP | Trang hợp lệ không phát sinh 500; 400/401/403/404/409/503 đúng ngữ cảnh, không đánh đồng với thành công |



Chạy lại các SQL/concurrency tests hiện có khi có môi trường, đặc biệt 20/50/100 request độc lập cùng ghế, DB double booking = 0. Không viết lại thuật toán chỉ để làm test dễ qua. Nếu chưa chạy thì giữ nguyên trạng thái chưa kiểm chứng trong \`CONCURRENCY_TEST_REPORT.md\` và \`PERFORMANCE_REPORT.md\`; không biến báo cáo cũ thành PASS.



Khi có browser automation, thử tương tác thật, đọc console/network và chụp screenshot website đang chạy: Home desktop/mobile, tìm chuyến, sơ đồ ghế, chi tiết vé, Login, Admin dashboard, một trang bảng/form Admin, Staff và Driver. Không dùng mockup làm screenshot. Dùng dữ liệu học tập, che thông tin cá nhân thật.



**## 14. Tài liệu và ZIP bàn giao**



Cập nhật tài liệu phản ánh source thực tế:



- \`README.md\`: mở Visual Studio, SDK .NET 9, SQL Server, Restore/Build/Run, hành vi đăng nhập theo role và địa chỉ các workspace.

- \`UI_CHANGELOG.md\`: CSS/JS/View/layout đã sửa, design system, responsive, animation, khác biệt trước/sau và giới hạn còn lại.

- \`LOGIN_REDIRECT_FIX.md\`: nguyên nhân, quy tắc vai trò/returnUrl, file thay đổi và testcase.

- \`DESIGN_REFERENCE_REVIEW\.md\`: nguồn thiết kế đã khảo sát và cách áp dụng có chọn lọc.

- \`ASSET_SOURCES.md\`: nguồn/quyền cho tài sản được thêm hoặc thay.

- \`FILE_CHANGES.md\`: danh sách file, mục đích sửa, phần backend tối thiểu cần đổi.

- \`TEST_RESULTS.md\`: lệnh/môi trường/ngày chạy, expected/actual, PASS/FAIL/CHƯA KIỂM CHỨNG, evidence và giới hạn.

- Giữ các tài liệu ERD, hold, state machine, realtime, permission matrix và traceability; chỉ sửa mục có liên quan thực sự. Giữ bản yêu cầu gốc.

- Nếu có \`SOURCE_MANIFEST.sha256\`, cập nhật theo nội dung gói bàn giao cuối.



Xuất **\*\*một ZIP source mới\*\***:



**\*\*\`QuanLyXeKhach_DatVe_GiuCho_REALTIME_UI_UPGRADED_REVIEWED.zip\`\*\***



Chỉ dùng **\*\*\`QuanLyXeKhach_DatVe_GiuCho_REALTIME_UI_UPGRADED_VERIFIED.zip\`\*\*** nếu thật sự restore/build/run với SQL Server và kiểm thử các luồng cốt lõi đã đạt; ghi riêng những nền tảng/viewport chưa thử. Không lấy kiểm tra tĩnh làm bằng chứng VERIFIED.



ZIP phải có solution/project .NET 9 hiện tại, source MVC, Controllers/Services, Identity, SQL Server migrations, worker/Hub, toàn bộ Razor/CSS/JS/ảnh/thư viện local cần thiết, seed/test và tài liệu. Không đóng gói \`bin\`, \`obj\`, \`.vs\`, \`node_modules\`, secrets, database riêng, bản backup hoặc dữ liệu cá nhân thật. Không đưa connection string chứa mật khẩu thật vào source.



Không gửi lại ZIP cũ hoặc một ZIP chỉ chứa tài liệu rồi báo đã nâng cấp website. Giữ cấu trúc dễ giải nén và mở \`BusBooking.sln\` bằng Visual Studio.



**## 15. Nội dung phản hồi cuối và tiêu chí hoàn thành**



Phản hồi cuối phải có:



1. Link tải ZIP source mới.

2. Tóm tắt giao diện đã nâng cấp trên đầy đủ nhóm trang khách/Admin/Staff/Driver.

3. Xác nhận quy tắc chuyển hướng Admin sau đăng nhập; nêu file đã sửa.

4. Kết quả restore/build/run, SQL Server, HTTP và browser với bằng chứng đã chạy; bảng PASS/FAIL/CHƯA KIỂM CHỨNG.

5. Screenshot thật nếu chạy được; nếu không, ghi rõ chưa có.

6. Hướng dẫn mở solution, cấu hình SQL Server, chạy lại trên Visual Studio và giữ database hiện có.

7. Giới hạn còn lại, không khẳng định đã hết HTTP 500 khi chưa kiểm thử.



**\*\*Thứ tự ưu tiên:\*\*** giữ hệ thống hoạt động và dữ liệu → bảo toàn đề tài/nghiệp vụ giữ chỗ 5 phút → sửa đăng nhập Admin → nâng toàn bộ giao diện trắng/xanh → dashboard tác nghiệp đầy đủ → responsive/accessibility → animation vừa đủ → kiểm thử và ZIP có thể mở lại.



**\*\*Hãy thực hiện trên source đính kèm đến khi có bản bàn giao cụ thể. Không xây lại project, không đổi tên đề tài, không bỏ nghiệp vụ, không thêm phạm vi mới và không chỉ làm đẹp trang chủ.\*\***


