# Nguồn điểm đón/trả — kiểm tra 03/11/2026

## Bổ sung dữ liệu mẫu ngày 04/11/2026

### Điều chỉnh lựa chọn sau phản hồi

Chạy `Database/20261104_AlignImportedStopChoices.sql` để bỏ hậu tố khỏi tên bến đầu và cuối, đồng thời ngừng đưa các điểm giữa đã nhập vào danh sách đặt mới. Điểm giữa còn thuộc lượt giữ hiệu lực được bảo lưu cho lượt đó. Script có thể chạy lại sau khi lượt giữ kết thúc. Vé đã xác nhận tiếp tục dùng thông tin đón/trả đã lưu lúc đặt.

Ba cặp hiện dùng cho các chuyến liên quan là Miền Đông mới → Liên tỉnh Đà Lạt, Liên tỉnh Đà Lạt → Miền Đông mới và Miền Đông mới → Liên tỉnh phía Nam Nha Trang. Giờ lấy từ lịch Bus Center, còn nguồn FUTA dùng để đối chiếu địa điểm.

Đã áp dụng 108 điểm cho 36 chuyến đang mở bán sử dụng xe TEST-001, TEST-002 và TEST-003. Các điểm mới mang nhãn **Mẫu đồ án** trên danh sách và vé. Bến là địa điểm tham khảo. Giờ và quyền chọn đón/trả thuộc kịch bản Bus Center.

| Chiều | Điểm đầu | Điểm giữa có thể chọn đón hoặc trả | Điểm cuối |
|---|---|---|---|
| TP.HCM → Đà Lạt | Bến xe Miền Đông mới | Bến xe Đức Long Bảo Lộc (+240 phút) | Bến xe Liên tỉnh Đà Lạt |
| Đà Lạt → TP.HCM | Bến xe Liên tỉnh Đà Lạt | Bến xe Đức Long Bảo Lộc (+180 phút) | Bến xe Miền Đông mới |
| TP.HCM → Nha Trang | Bến xe Miền Đông mới | Bến xe Cam Ranh (+330 phút) | Bến xe Liên tỉnh phía Nam Nha Trang |

Nguồn đối chiếu tên bến và vị trí gồm [FUTA về các tuyến từ Miền Đông mới](https://futabus.vn/tin-tuc/ve-mien-tay-ra-mien-trung-den-tay-nguyen-voi-28-tuyen-xe-di-tu-ben-xe-mien-dong-moi), [FUTA về Bảo Lộc](https://futabus.vn/tin-tuc/cong-ty-phuong-trang-thong-bao-mo-tuyen-moi-ben-xe-duc-long-bao-loc-ben-xe-quang-dien-hue), [FUTA về Cam Ranh](https://futabus.vn/tin-tuc/nha-trang-cam-ranh-phan-rang-xe-buyt-them-tuyen-hanh-trinh-them-tien) và các danh bạ, thông báo bến ở mục dưới. Luồng tra cứu tương tác của FUTA chưa xác minh được danh sách điểm theo từng giờ chuyến.

## Khảo sát ban đầu ngày 03/11/2026

Đây là **hồ sơ đối chiếu địa điểm**, không phải lịch chính thức hoặc chứng nhận Bus Center có quyền khai thác. Không trộn văn phòng hai hãng rồi gọi là một tuyến Bus Center. Không import các ứng viên dưới đây vào DB một cách tự động.

| Địa điểm tham khảo | Địa chỉ đối chiếu | Nguồn/đơn vị | Giới hạn |
|---|---|---|---|
| Bến xe Miền Đông mới | 501 Hoàng Hữu Nam, phường Long Bình, TP.HCM | SAMCO: https://samco.com.vn/vn/ben-xe-mien-dong-moi.html ; bến: https://benxemiendongmoi.com.vn/Post/Detail/s/phuong-tien-ket-noi-ben-xe-mien-dong-moi | Nội dung search-index chính thức có địa chỉ; lần mở trực tiếp502. Chưa xác nhận quầy/cổng/quyền đón/trả Bus Center |
| Bến xe Liên tỉnh Đà Lạt | 01 Tô Hiến Thành, Đà Lạt, Lâm Đồng | Bến xe Huế, danh bạ bến đối lưu: https://benxehue.vn/ben-xe-doi-luu/ben-xe-lien-tinh-da-lat | Mở trang được. Trang có cả tên phường cũ/mới ở các mục; cần đối chiếu địa chỉ hành chính khi triển khai, không suy quyền khai thác Bus Center |
| Bến xe Liên tỉnh phía Nam phục vụ khu vực Nha Trang | Đại lộ Võ Nguyên Giáp, thôn Trung Nam, Khánh Hòa; thông báo2024 ghi xã Diên Toàn, huyện Diên Khánh | Thông báo của Công ty CP Khai thác bến xe và DVVT Khánh Hòa đăng tại https://baokhanhhoa.vn/rao-vat-tuyen-dung/khac/202403/thong-bao-khai-truong-ben-xe-lien-tinh-phia-nam-27819e7/ ; nguồn cơ quan tài trợ https://qdtpt.khanhhoa.gov.vn/vi-vn/danh-sach-tin/categoryid/tintuc/pg/4 | Bến mới thay địa chỉ km6 đường23/10 từ01/04/2024; địa chỉ hành chính có thay đổi sau đó, nguồn operator https://dichvuvantaikh.vn/ có footer index mới nhưng mở homepage timeout. Chưa xác nhận điểm trả/cổng/quầy của Bus Center |

Không đưa số điện thoại các đơn vị trên vào Liên hệ Bus Center. Không dùng địa chỉ km6 đường23/10 cũ cho bến mới mà không đối chiếu. Các nguồn nêu tên/vị trí bến, không chứng minh xe của chuyến hiện tại đến đúng đó hoặc được đón/trả ở mọi nơi trong khu vực.

## Khảo sát theo chiều

| Chiều đang có | Điểm đầu/cuối có thể đối chiếu | Thời gian giữ | Trạng thái áp dụng |
|---|---|---|---|
| TP.HCM → Đà Lạt | Miền Đông mới ↔ Liên tỉnh Đà Lạt, chỉ là ứng viên hai đầu | DepartureUtc/ArrivalUtc gốc; không lấy giờ FUTA | Chờ chủ vận hành xác nhận bến/quầy/quyền khai thác, Admin nhập |
| Đà Lạt → TP.HCM | Cùng địa điểm ứng viên theo chiều ngược, không đảo offset chiều đi | Giờ chuyến chiều về riêng | Chờ duyệt độc lập |
| TP.HCM → Nha Trang | Miền Đông mới ↔ Liên tỉnh phía Nam; phải kiểm khu vực đến/điểm trả thực | Giờ chuyến gốc; không nhận cổng bến ở Diên Khánh là địa chỉ trung tâm Nha Trang | Chờ duyệt địa điểm/địa chỉ hiện hành và quan hệ khai thác |

Các địa phương Bảo Lộc/Đức Trọng/Cam Ranh/Cam Lâm trong template là tên khu vực. Không có căn cứ bến/văn phòng/điểm hẹn hoặc quyền dừng để tự mở cả đón lẫn trả. Bản mới không cho các địa phương mẫu đó vào lựa chọn mới, kể cả cờ raw đang true; không UPDATE cờ hoặc xóa điểm cũ. Hold cũ vẫn giữ ID hợp lệ của nó.

## Căn cứ giờ và cờ nội bộ

Template đầu vào vẫn được giữ làm provenance: TP.HCM→Đà Lạt420 phút (Bảo Lộc240/ĐứcTrọng330); chiều về420 (ĐứcTrọng90/BảoLộc180); TP.HCM→NhaTrang480 (CamRanh330/CamLâm400). Đây là cấu hình học tập, chưa phải giờ xe Bus Center được xác minh. Không tự bẻ chúng thành bến thật; lệnh cũ không hoàn thành yêu cầu địa điểm cụ thể mới.

Điểm cụ thể mới do Admin duyệt: offset0 chỉ đón ở khu vực xuất phát, offsetDuration chỉ trả ở khu vực đến; điểm giữa có offset/cờ được người vận hành xác nhận riêng. Area là khu vực phục vụ của tuyến do Admin xác nhận, không phải geocoding tự động. SourceReference ghi URL địa điểm và căn cứ khai thác/chuyến. IsIllustrative của bản ghi mới vẫn true để không tự nhận giờ cấu hình là vận hành được bên ngoài chứng nhận; source/cờ cũ và snapshot không sửa.

Không có kết nối DB người dùng trong phiên này. Chưa biết TripId thật hoặc điểm custom đã duyệt của máy bạn, không xuất báo cáo giả “đã sửa đúng toàn bộ”. Source đã thêm công cụ quản trị/cập nhật có kiểm soát; dữ liệu cụ thể cần chủ vận hành cung cấp và duyệt theo STOP_DATA_UPDATE_GUIDE.md.
