-- Thay các cặp tài xế đồ án cũ bằng hồ sơ đã thêm trong 20261105_SeedDriverProfiles.sql.
-- Đặt @Apply = 0 để kiểm tra toàn bộ kế hoạch và hoàn tác cuối lệnh.
SET XACT_ABORT ON;
DECLARE @Apply bit=1, @ScheduleBufferMinutes int=0;
BEGIN TRANSACTION;

IF NOT EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId=N'20261105000200_AddDriverProfileAndVehicleType'
)
    THROW 50001,N'Cần cập nhật hồ sơ tài xế trước khi phân công.',1;

DECLARE @LockResult int;
EXEC @LockResult=sp_getapplock @Resource=N'BusBooking.Catalog',
    @LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000;
IF @LockResult<0
    THROW 50002,N'Danh mục đang được cập nhật. Hãy thử lại.',1;

DECLARE @Now datetime2=SYSUTCDATETIME();
DECLARE @VehicleCrews table (Plate nvarchar(30) PRIMARY KEY, MainNumber int);
INSERT @VehicleCrews(Plate,MainNumber) VALUES
    (N'FLEET-01-1',1),(N'FLEET-01-2',3),
    (N'FLEET-02-1',5),(N'FLEET-02-2',7),
    (N'FLEET-03-1',9),(N'FLEET-03-2',11),
    (N'FLEET-04-1',13),(N'FLEET-04-2',15),
    (N'FLEET-05-1',17),(N'FLEET-05-2',19),
    (N'FLEET-06-1',21),(N'FLEET-06-2',23),
    (N'FLEET-07-1',25),(N'FLEET-07-2',27),
    (N'FLEET-08-1',29),(N'FLEET-08-2',31),
    (N'FLEET-09-1',33),(N'FLEET-09-2',35);

DECLARE @EligibleDrivers table (Id nvarchar(450) PRIMARY KEY);
INSERT @EligibleDrivers(Id)
SELECT u.Id
FROM dbo.AspNetUsers u
JOIN dbo.AspNetUserRoles ur ON ur.UserId=u.Id
JOIN dbo.AspNetRoles r ON r.Id=ur.RoleId
WHERE u.IsActive=1 AND r.NormalizedName=N'DRIVER'
    AND u.Id LIKE N'f3aad073-591b-450f-a711-%';

DECLARE @Plan table (
    TripId int PRIMARY KEY, RouteId int, DepartureUtc datetime2, ArrivalUtc datetime2,
    PreviousDriverId nvarchar(450), PreviousCoDriverId nvarchar(450),
    DriverId nvarchar(450), CoDriverId nvarchar(450)
);
INSERT @Plan(TripId,RouteId,DepartureUtc,ArrivalUtc,PreviousDriverId,PreviousCoDriverId,DriverId,CoDriverId)
SELECT t.Id,t.BusRouteId,t.DepartureUtc,t.ArrivalUtc,t.DriverId,t.CoDriverId,
       CONCAT(N'f3aad073-591b-450f-a711-',RIGHT(CONCAT('000000000000',vc.MainNumber),12)),
       CONCAT(N'f3aad073-591b-450f-a711-',RIGHT(CONCAT('000000000000',vc.MainNumber+1),12))
FROM dbo.Trips t WITH (UPDLOCK)
JOIN dbo.Vehicles v ON v.Id=t.VehicleId
JOIN @VehicleCrews vc ON vc.Plate=v.Plate
JOIN dbo.AspNetUsers d ON d.Id=t.DriverId
JOIN dbo.AspNetUsers c ON c.Id=t.CoDriverId
WHERE t.Status=N'Scheduled' AND t.DepartureUtc>@Now
  AND d.NormalizedEmail=CONCAT(N'FLEET-DRIVER-',RIGHT(CONCAT('00',vc.MainNumber),2),N'@BUSBOOKING.TEST')
  AND c.NormalizedEmail=CONCAT(N'FLEET-DRIVER-',RIGHT(CONCAT('00',vc.MainNumber+1),2),N'@BUSBOOKING.TEST');

IF EXISTS (
    SELECT 1 FROM @Plan p
    LEFT JOIN @EligibleDrivers d ON d.Id=p.DriverId
    LEFT JOIN @EligibleDrivers c ON c.Id=p.CoDriverId
    WHERE d.Id IS NULL OR c.Id IS NULL OR d.Id=c.Id
)
    THROW 50003,N'Một cặp tài xế mới thiếu hồ sơ hoạt động hoặc không hợp lệ.',1;

IF EXISTS (
    SELECT 1 FROM @Plan p
    JOIN dbo.Trips other ON other.Id<>p.TripId AND other.Status<>N'Cancelled'
        AND other.DepartureUtc<DATEADD(minute,@ScheduleBufferMinutes,p.ArrivalUtc)
        AND other.ArrivalUtc>DATEADD(minute,-@ScheduleBufferMinutes,p.DepartureUtc)
    LEFT JOIN @Plan replacement ON replacement.TripId=other.Id
    WHERE COALESCE(replacement.DriverId,other.DriverId) IN (p.DriverId,p.CoDriverId)
       OR COALESCE(replacement.CoDriverId,other.CoDriverId) IN (p.DriverId,p.CoDriverId)
)
    THROW 50004,N'Phân công mới bị trùng lịch. Dữ liệu chưa được cập nhật.',1;

DECLARE @Changed table (
    TripId int PRIMARY KEY, PreviousDriverId nvarchar(450), PreviousCoDriverId nvarchar(450),
    DriverId nvarchar(450), CoDriverId nvarchar(450)
);
UPDATE t
SET DriverId=p.DriverId, CoDriverId=p.CoDriverId, Revision=t.Revision+1
OUTPUT inserted.Id,deleted.DriverId,deleted.CoDriverId,inserted.DriverId,inserted.CoDriverId
INTO @Changed(TripId,PreviousDriverId,PreviousCoDriverId,DriverId,CoDriverId)
FROM dbo.Trips t
JOIN @Plan p ON p.TripId=t.Id
WHERE t.Status=N'Scheduled' AND t.DepartureUtc>SYSUTCDATETIME()
  AND t.DriverId=p.PreviousDriverId AND t.CoDriverId=p.PreviousCoDriverId;

IF (SELECT COUNT(*) FROM @Plan)<>(SELECT COUNT(*) FROM @Changed)
    THROW 50005,N'Lịch chuyến vừa thay đổi. Hãy lập lại kế hoạch.',1;

INSERT dbo.AuditEntries(ActorId,Action,EntityId,CreatedUtc,Detail)
SELECT N'driver-profile-seed',N'TripCrewReassigned',CONVERT(nvarchar(100),TripId),@Now,
       CONCAT(N'Previous=',PreviousDriverId,N',',PreviousCoDriverId,N'; Current=',DriverId,N',',CoDriverId)
FROM @Changed;

SELECT @Apply AS Applied, COUNT(*) AS ChangedTrips, COUNT(DISTINCT RouteId) AS Routes,
       COUNT(DISTINCT DriverId)+COUNT(DISTINCT CoDriverId) AS NewDriversUsed
FROM @Plan;

IF @Apply=1 COMMIT;
ELSE ROLLBACK;
