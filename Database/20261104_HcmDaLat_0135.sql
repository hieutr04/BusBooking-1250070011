SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @lock int;
EXEC @lock=sp_getapplock @Resource=N'BusBooking.Catalog', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000;
IF @lock<0 THROW 50001,N'Không lấy được khóa lịch chuyến.',1;

DECLARE @tripId int=NULL; -- NULL: áp dụng cho các chuyến phù hợp; điền ID để giới hạn một chuyến.
DECLARE @apply bit=0;     -- Chuyển thành 1 sau khi xem trước.
DECLARE @points TABLE(Seq int PRIMARY KEY, Name nvarchar(160), Address nvarchar(500), OffsetMinutes int, Pickup bit);
-- Phút tính từ 01:35, theo danh sách người dùng cung cấp.
INSERT @points VALUES
(0,N'Bến xe Miền Đông Mới',N'39448 Xa Lộ Hà Nội, Phường Long Bình, TP Thủ Đức, TP.HCM',0,1),
(1,N'Suối Linh',N'Số D9, KP 4, Xa lộ Hà Nội, Phường Long Bình, Đồng Nai',15,1),
(2,N'Trảng Bom',N'17 Thanh Hóa, Hố Nai 3, Trảng Bom, Đồng Nai',15,1),
(3,N'Phòng vé Phúc Lộc - Đồng Nai',N'2559+5H2, QL20, Quang Trung, Thống Nhất, Đồng Nai',75,1),
(4,N'Tân Phú',N'782 QL20, Tân Phú, Đồng Nai',220,1),
(5,N'Phương Lâm',N'2421 QL20, Phú Lâm, Tân Phú, Đồng Nai',250,1),
(6,N'Bảo Lộc',N'399 Trần Phú, Lộc Sơn, Bảo Lộc, Lâm Đồng',360,0),
(7,N'Lộc An',N'QL20, Lộc An, Bảo Lộc, Lâm Đồng',375,0),
(8,N'Hòa Ninh',N'Ngã ba Hòa Ninh, QL20–30, Thôn 2, Hòa Ninh, Di Linh, Lâm Đồng',380,0),
(9,N'Di Linh',N'Đường Hùng Vương, Di Linh, Lâm Đồng',400,0),
(10,N'Tà Hine',N'QL20, Ninh Gia, Đức Trọng, Lâm Đồng',420,0),
(11,N'Đức Trọng',N'795 Quốc lộ 20, Liên Nghĩa, Đức Trọng, Lâm Đồng',435,0),
(12,N'Thạnh Mỹ',N'44 đường 2/4, Thạnh Mỹ, Đơn Dương, Lâm Đồng',450,0),
(13,N'Fi Nôm',N'Văn phòng Phương Trang Fi Nôm, Tổ 13 QL20, Phi Nôm, Hiệp Thạnh, Lâm Đồng',450,0),
(14,N'Bến xe Đà Lạt',N'1 Đường Tô Hiến Thành, Phường 3, Đà Lạt, Lâm Đồng',480,0),
(15,N'Lạc Dương - Đà Lạt',N'3 đường Langbiang, Lạc Dương, Lâm Đồng',540,0),
(16,N'Lê Quý Đôn',N'11A/2 Lê Quý Đôn, Phường 1, Đà Lạt, Lâm Đồng',540,0),
(17,N'Trại Mát',N'11 Huỳnh Tấn Phát, Phường 11, Đà Lạt, Lâm Đồng',540,0),
(18,N'Xô Viết Nghệ Tĩnh',N'53 Xô Viết Nghệ Tĩnh, Đà Lạt, Lâm Đồng',540,0),
(19,N'D''Ran',N'31 Trần Hưng Đạo, D''Ran, Đơn Dương, Lâm Đồng',570,0);

DECLARE @candidates TABLE(Id int PRIMARY KEY, StartUtc datetime2, EndUtc datetime2, VehicleId int, DriverId nvarchar(450), StaffId nvarchar(450));
INSERT @candidates
SELECT t.Id,shift.StartUtc,DATEADD(minute,570,shift.StartUtc),t.VehicleId,t.DriverId,t.StaffId
FROM dbo.Trips t WITH (UPDLOCK,ROWLOCK)
JOIN dbo.Routes r ON r.Id=t.BusRouteId
CROSS APPLY (SELECT DATEADD(minute,-325,CONVERT(datetime2,CONVERT(date,DATEADD(hour,7,t.DepartureUtc)))) AS StartUtc) shift
WHERE r.Origin=N'TP. Hồ Chí Minh' AND r.Destination=N'Đà Lạt'
AND t.Status='Scheduled' AND t.SalesOpen=1 AND (@tripId IS NULL OR t.Id=@tripId)
AND shift.StartUtc>SYSUTCDATETIME()
AND NOT EXISTS(SELECT 1 FROM dbo.SeatHolds h WHERE h.TripId=t.Id)
AND NOT EXISTS(SELECT 1 FROM dbo.Tickets x JOIN dbo.TripSeats s ON s.Id=x.TripSeatId WHERE s.TripId=t.Id)
AND NOT EXISTS(SELECT 1 FROM dbo.TripStops s WHERE s.TripId=t.Id AND s.IsActive=1 AND s.Address IS NOT NULL
 AND s.Name NOT IN (N'Bến xe Miền Đông mới',N'Bến xe Liên tỉnh Đà Lạt'))
AND NOT EXISTS(SELECT 1 FROM dbo.AuditEntries a WHERE a.Action=N'HcmDaLat0135Imported' AND a.EntityId=CONVERT(nvarchar(100),t.Id));

DELETE c FROM @candidates c
WHERE EXISTS(
    SELECT 1 FROM dbo.Trips t
    WHERE t.Id<>c.Id AND t.Status<>'Cancelled' AND t.DepartureUtc<c.EndUtc AND t.ArrivalUtc>c.StartUtc
    AND (t.VehicleId=c.VehicleId OR t.DriverId=c.DriverId OR t.StaffId=c.StaffId)
) OR EXISTS(
    SELECT 1 FROM @candidates other
    WHERE other.Id<>c.Id AND other.StartUtc<c.EndUtc AND other.EndUtc>c.StartUtc
    AND (other.VehicleId=c.VehicleId OR other.DriverId=c.DriverId OR other.StaffId=c.StaffId)
);

IF @apply=1
BEGIN
    DELETE s FROM dbo.TripStops s JOIN @candidates c ON c.Id=s.TripId;
    UPDATE t SET DepartureUtc=c.StartUtc,ArrivalUtc=c.EndUtc,Revision=t.Revision+1
    FROM dbo.Trips t JOIN @candidates c ON c.Id=t.Id;
    INSERT dbo.TripStops(TripId,StopOrder,Name,Address,OffsetMinutes,CanPickup,CanDropoff,IsActive,IsIllustrative)
    SELECT c.Id,CASE WHEN p.Seq=19 THEN 1000 ELSE p.Seq*10 END,p.Name,p.Address,p.OffsetMinutes,p.Pickup,1-p.Pickup,1,0
    FROM @candidates c CROSS JOIN @points p;
    INSERT dbo.AuditEntries(CreatedUtc,ActorId,Action,EntityId,Detail)
    SELECT SYSUTCDATETIME(),N'command',N'HcmDaLat0135Imported',CONVERT(nvarchar(100),Id),
           N'6 pickup, 14 dropoff; timetable supplied by user; 01:35–11:05 UTC+7' FROM @candidates;
END;

SELECT Id,DATEADD(hour,7,StartUtc) AS DepartureVN,DATEADD(hour,7,EndUtc) AS LastStopVN,@apply AS Applied
FROM @candidates ORDER BY Id;
COMMIT;
