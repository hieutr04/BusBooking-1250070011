# Kiểm tra BusGo trên app thật

- Build/Razor/DI: chạy lệnh trong TEST_RESULTS. Kiểm không có model pending ngoài3 migration có sẵn.
- SQL chỉ DB riêng `_Tests`; SqlTests có --planner-only không ghi DB và case append thực nằm trong bộ --ack-test-db. Giữ raw kết quả, không sửa PASS trước.
- Trên app test: Admin/Chuyến/Điểm đón trả → Scheduled tương lai chỉ endpoint; có activehold, expiredhold và bookingconfirmed; append không đổi chọn/expiry/snapshot. Lặp không thêm hàng/revision/audit. Xung đột tên/offset/gap/20điểm/status/departurepast trảlýdo; stale revision409. Chặn POST khôngCSRF/rolekhác. Hai Admin cập nhật đồng thời chỉ một kết quả commit; công bố revision sau commit.
- Chuyến mới ĐàLạt→TP.HCM420 nhận4điểm; chuyến tổngkhác không nội suy. Điểm custom cũ đúng hoặc xung đột phải được giữ. Chưa có địa chỉ thì không hiện placeholder; không nhận điểm là văn phòng BusGo.
- Hai Customer tranh cùngghế; giữ300giây/noextension, chọnđiểm→refresh→multi-hold→confirm→snapshot vé. WrongTripId/inverted/disabled bị từchối; fare toànchuyến; release/confirm tokencũ khôngảnhhưởng người sau. Đổi vé/hủy/thu/hoàn/racegiữ harness gốc.
- Toàn bộ View liệt kê UI_PAGE_AUDIT; login4role, Account/Profile, Home/Trips/Bookings/Contact/Photos/Policy và mọi bảng/form Admin, Staff, Driver. Rà marker học tập/mô phỏng/brandcũ trên DOM sau dữ liệu động, kể cả vé cũ. Giữ nguồnảnh; tênkỹthuậttrongcode không phải lỗibrand.
- Viewports375/390/768/1024/1440: khôngtrànngangtoàntrang, bảngcuộntrongkhung; menu/sidebar Escape/focus,keyboard,zoom200%,reducedmotion; khôngconsoleerror hoặcnetwork500. Screenshot chỉ từapp MVCđangchạy với datagiả, khôngHTMLfixture.

DOM test không cầnSQL: npm install tại Tests rồi npm run seat-ui và npm run stop-editor. Source review: pip install tree-sitter tree-sitter-c-sharp tinycss2 rồi python Tests/busgo-review.py --baseline <ZIPđầuvào>. Không cần Node/Python để chạy website.
