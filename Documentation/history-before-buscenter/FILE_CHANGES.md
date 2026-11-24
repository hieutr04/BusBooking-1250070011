# BusGo — thay đổi so với ZIP STOPS_SEATS đầu vào

Danh sách đầy đủ kèm trạng thái NEW/MODIFIED/UNCHANGED và SHA256 trước/sau ở FILE_CHANGES.csv. Không có file đầu vào bị xóa. CSV tự tham chiếu được đánh dấu GENERATED thay vì checksum vòng lặp.

- `Services/StopPresentation.cs`, `AuditPresentation.cs`: đầu ra tên/địa chỉ/thời lượng/nhật ký; giữ dữ liệu raw.
- `Services/StopAppendPlanner.cs`, `TripStopAppendService.cs`, `DalatStopMaintenance.cs`, `ViewModels/StopAppendForms.cs`: planning, preview, insert-only, report.
- `AdminController`, `Views/Admin/TripStops`, `stop-editor.js`: Admin duyệt/bổ sung riêng, full editor vẫn khóa lịch có hold. RouteEdit giữ sentinel nội bộ khi form sạch được gửi nguyên.
- `CatalogService`: chỉ chuyến mới đúng chiều420 phút nhận điểm mặc định; `TripStopMaintenance` chuyển chiều này sang nhánh chỉ-thêm. `Program`: DI/command, lỗi hiển thị BusGo; không đổi cookie/SQL/profile/auth.
- `TripStopService`: ToView/Edit dùng nhãn sạch, fullSave giữ raw khi người dùng không đổi nhãn, khóa thêm điều kiện chưa đến giờ đi. Select/Resolve validation giữ nguyên byte.
- `BookingService`: đúng3 câu lỗi người dùng; phần còn lại giữ nguyên. ExceptionFilter/Home/Contact: copy lỗi gọn, status/logging vẫn giữ.
- View các nhóm công khai/Admin/Staff/Driver và layout/partial; `busgo.css`, brand vector/favicon/PhotoCatalog sở hữu. Tất cả trang dùng style chung; kiểm browser vẫn chưa thực hiện. Ảnh Commons/giấy phép giữ nguyên.
- Tests: mở rộng DOM, thêm stop-editor DOM, planner C# và SQL regression; giữ tests cũ. Tài liệu hướng dẫn/kiểm chứng mới, yêu cầu gốc nguyên byte và tài liệu cũ lưu history-before-busgo.

Bằng chứng giữ nguyên .NET9/cấu hình, Models/DbContext/migrations, worker/hub/auth/SQL gate và ảnh ở Documentation/validation/busgo-source-review.json. Runtime chưa kiểm chứng xem TEST_RESULTS.md.
