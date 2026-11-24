# Báo cáo tranh chấp ghế — CHƯA KIỂM CHỨNG

Không có SQL Server/runtime khả dụng. Không điền0 double booking khi chưa đo.

| HTTP đồng thời | Gửi/hoàn thành | Thắng mới | Conflict hợp lệ | Lỗi bất ngờ | Hold/vé hiệu lực sau đợt | Double booking | Trạng thái |
|---|---|---|---|---|---|---|---|
| 20 | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa đo | CHƯA KIỂM CHỨNG |
| 50 | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa đo | CHƯA KIỂM CHỨNG |
| 100 | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa đo | Chưa đo | CHƯA KIỂM CHỨNG |

## Thực thi

`dotnet run --project Tests/BusBooking.SqlTests -- --ack-test-db --http http://localhost:5190 --five-minutes`

- SQL_TEST_CONNECTION chỉ DB riêng kết thúc `_Tests`, cùng DB app HTTP. Không drop/reset dữ liệu người dùng.
- Mỗi đợt tạo Trip mới/ghế hiện hữu, tài khoản UserManager hợp lệ.20 gồm19 Customer+1 Staff;50/100 là Customer độc lập.
- CookieContainer/login/CSRF mỗi session, barrier khởi phát sau khi đăng nhập xong, mỗi logical request key mới.
- Raw status/body/ms.200 chỉ một,409 còn lại;401/403/400/5xx/network là lỗi phải điều tra, không bằng chứng chống trùng.
- SQL đối chiếu: hold Active chưa quá hạn, ticket IsActive, nhóm trùng ticket. Test hold mong đợi một hold, chưa ticket mới; test chức năng kiểm Ticket↔Booked.
- Nếu kéo qua hạn300s, assertion active hold thất bại, không được nhận là đợt hợp lệ.
- JSON/CSV từng đợt ở TestResults/<run>/concurrency-N.*. Không có raw đo thật kèm ZIP vì chưa chạy.

Có thêm tests confirm/worker, close/confirm, token cũ, all-or-nothing, filtered unique index, rollback sau insert wait. Browser nhiều client/reconnect. Chưa bao phủ mọi interleaving hoặc có kết quả thực.
