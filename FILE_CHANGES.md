# Thay đổi bản Bus Center so với ZIP BusGo đầu vào

FILE_CHANGES.csv có mọi đường dẫn, NEW/MODIFIED/UNCHANGED và SHA256 trước/sau; không xóa file đầu vào. Checksum của chính CSV được loại để tránh vòng tự tham chiếu.

- Brand/layout/ownedSVG/favicon/PhotoCatalogNeutral/Program fallback: Bus Center; tên kỹ thuật/cookie/role/route/config giữ nguyên. Thêm buscenter-mark.svg, buscenter.css.
- Home/Index, _Photo, CSSphotos-contact/busgo: hero hai cột16:9+search, bỏ caption/badge/cardcalc; không tạo banner khác rồi bỏ nguyên vùng cũ.
- ContactOptions thêm LegalName/Website/Hours và kiểm URL/phone; ContactView/_SupportChannels/footer dùng chung cấu hình, template và guide. ContactController/Service/Tokens/forms/SQL/CSRF/rate/PRG/Admin giữ nguyên.
- PhotosView/PhotoCatalog: ghi công tập trung đầy đủ title/author/source/license/modifications; WebP nguyên byte, logo ảnh không sửa, footer công khai.
- StopChoicePolicy: chọn mới loại địa phương mẫu giữa đường và chuyển endpoint mặc định khi có địa điểm cụ thể; giữ chính lựa chọn cũ của holder.
- SpecificStopPlanner/SpecificStopMaintenance/SpecificStopForm; TripStopAppendService.Specific/AdminController.SpecificStop/TripStopsView/stop-editor: duyệt dữ liệu cụ thể, append endpoint phiên bản bằng order trống, sourceaudit, dry-run/revision/batchreport; không UPDATE điểm cũ.
- TripStopService thêm wrapper ResolveNew/ResolveHeld, endpoint mới giữ exactArrival; Select/Resolve cũ nguyên byte. BookingService chỉ đổi point eligibility/snapshot, mọi phần còn lại khớp baseline. StopView thêm CanSelect/LocationSpecific DTO, không cột DB. seats.js lọc retainedpoint theo chính hold và link xác nhận địa điểm chưa cụ thể, không thay timer/fare/payloadinventory/confirm.
- Tests: thêm policy/contactoptions/C#SQLregression; mở rộng DOMretained/default/pointpreview; thêm contact-dom và buscenter-browser thật. Không đổi báo cáo cũ thành lần này. Source review mới buscenter-review.py; busgo-review là baseline cũ.
- Docs yêu cầu gốc giữ nguyên, thêm promptmới; README/CHANGELOG/TESTRESULTS/CONTACT/STOP/DESIGN/ASSETS cập nhật và lưu history-before-buscenter. Không có migration, thay connstring hoặc seed tự động.

Thông tin vận hành còn thiếu: liên hệ thật và quyền/lịch khai thác điểm Bus Center. Không tự gán ứng viên từ hãng khác, không có báo cáo DB người dùng hoặc screenshot MVC thực. Xem TEST_RESULTS.md.
