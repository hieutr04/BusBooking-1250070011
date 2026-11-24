# Kết quả kiểm thử bàn giao — 29/10/2026, giờ Việt Nam

**Chưa VERIFIED. Không có kết quả vận hành trên SQL Server.**

## Môi trường

Sandbox Linux x64 có Node/Python, không có dotnet ban đầu, SQL Server/docker/sqlcmd/Visual Studio. Đã tải SDK9.0.305/runtime9.0.9 để thử. Host liệt kê được thư mục SDK/runtime nhưng khởi tạo CoreCLR thất bại: HRESULT0x8007000E, exit137 trước MSBuild/restore/app.

Lệnh đã thử và stdout/exit code: Documentation/validation/runtime-attempts.json. Không suy ra SDK/VS trên máy người dùng từ môi trường này. Không dùng SQLite/InMemory để “kiểm thay” SQL locks.

| Nhóm | Kết quả | Bằng chứng / giới hạn |
|---|---|---|
| Solution/project/JSON/local assets | Xem static-checks.json | File/config checks, không build |
| C# grammar bằng tree-sitter | Xem csharp-syntax.json | Không type-check/Razor/DI |
| JS Node --check | Xem static-checks.json | Cú pháp, không browser |
| --info/restore/build/run | CHƯA KIỂM CHỨNG (bị chặn) | CoreCLR init thất bại |
| EF snapshot/migration SQL | CHƯA KIỂM CHỨNG | Chưa so model/DDL SQL |
| Seed/Identity bốn role | CHƯA KIỂM CHỨNG | Có UserManager/RoleManager, chưa chạy |
| HTTP/Controller/API/Razor/DI/form/status | CHƯA KIỂM CHỨNG | App chưa khởi động |
| RBAC/IDOR/CSRF | CHƯA KIỂM CHỨNG | SQL/browser test chờ chạy |
| Giữ/xác nhận/nhả/giá server | CHƯA KIỂM CHỨNG | SQL harness |
| Giữ đủ300 giây thực | CHƯA KIỂM CHỨNG | --five-minutes chờ302s, chưa chạy |
| Expiry/worker/owner/token cũ/retry | CHƯA KIỂM CHỨNG | Fixture SQL + race tests chưa chạy |
| Restart process thực | CHƯA KIỂM CHỨNG | Checklist Tests/README, scope mới không tương đương |
| Hủy/đổi/rollback sát hạn/thu hoàn | CHƯA KIỂM CHỨNG | Harness và trigger fixture chưa chạy |
| Close/cancel chuyến đua confirm | CHƯA KIỂM CHỨNG | Service SQL tests |
| 20/50/100 HTTP tranh ghế | CHƯA KIỂM CHỨNG | Không có số double booking thực |
| SignalR/reconnect/disconnect | CHƯA KIỂM CHỨNG | Browser + RTT metrics |
| Mean/P95/SignalR RTT | CHƯA KIỂM CHỨNG | Không có mẫu đo |
| Responsive375/768/1024/1440 | CHƯA KIỂM CHỨNG | Có CSS/test, chưa browser QA |
| ZIP CRC/exclusions/manifest | Xem packaging.json | Chỉ bao gói, không runtime |
| Word/ảnh nhận xét | CHƯA ĐỐI CHIẾU | Không có attachments hai nguồn này |

## Đã sửa trong rà source

- renderedHoldId riêng để render form hành khách ngay khi acquire thành công.
- Conditional SQL UPDATE hạn cuối sau insert/index waits; rollback cả đơn/vé mới và vé đổi cũ khi hết hạn.
- Worker batch50; Acquire reclaim hold quá hạn chạm ghế yêu cầu, không phụ thuộc worker.
- no-store đặt sau authentication; status503 cho SQL lỗi ngoài controller.
- Rà test lambda, migration ID, mutation binding và assets/license local.

Đây là source review, không phải xác nhận lỗi runtime đã hết. Chạy verify.ps1, SQL harness và browser test theo Tests/README để bổ sung kết quả có raw logs.
