# Activity: giữ và xác nhận

```mermaid
flowchart TD
    A["Đăng nhập, chọn ghế"] --> B["Khóa Trip và ghế trong transaction"]
    B --> C{"Tất cả trống hoặc hold cũ quá hạn?"}
    C -->|Không| D["Rollback, trả 409 và resync"]
    C -->|Có| E["Lưu hold 300s, commit rồi SignalR"]
    E --> F["Khách nhập hành khách ngoài transaction"]
    F --> G{"SQL UPDATE đúng owner, còn hạn, chuyến hợp lệ?"}
    G -->|Có| H["Đơn/vé và Booked cùng commit"]
    G -->|Không| I["Rollback mọi thay đổi"]
```

## Hủy / đổi

```mermaid
flowchart TD
    A["Chủ vé gửi yêu cầu trước khởi hành"] --> B{"Nhân viên xử lý"}
    B -->|Từ chối| C["Lưu lý do; vé không đổi"]
    B -->|Hủy| D["Khóa Trip, vô hiệu vé và nhả ghế"]
    B -->|Đổi| E["Giữ ghế mới 300s qua service chung"]
    E --> F{"Xác nhận thay thế hợp lệ?"}
    F -->|Không| G["Rollback; giữ nguyên vé cũ"]
    F -->|Có| H["Vé cũ Exchanged, vé mới Active nguyên tử"]
    D --> I["Khoản đã thu chờ đối soát trực tiếp"]
    H --> I
```

Worker: quét index theo batch → khóa Trip → kiểm hạn SQL lại → chỉ nhả đúng CurrentHold Active quá hạn → commit → invalidation. Restart không khởi tạo lại hạn.
