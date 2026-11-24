-- Dùng lịch và đội xe hiện có để bổ sung các ngày chưa có chuyến trên từng tuyến.
-- @Apply=0 chỉ xem kế hoạch. Chuyển thành 1 để ghi toàn bộ trong cùng giao dịch.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
SET NOCOUNT ON;

DECLARE @Apply bit=0, @ScheduleBufferMinutes int=0;
DECLARE @StartDate date='20261115', @EndDate date='20261231';
BEGIN TRANSACTION;

DECLARE @LockResult int;
EXEC @LockResult=sys.sp_getapplock @Resource=N'BusBooking.Catalog',
    @LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000;
IF @LockResult<0
    THROW 50001,N'Không lấy được khóa để lập lịch chuyến.',1;

DECLARE @Now datetime2=SYSUTCDATETIME();
DECLARE @Tomorrow date=DATEADD(day,1,CONVERT(date,DATEADD(hour,7,@Now)));
IF @StartDate<@Tomorrow SET @StartDate=@Tomorrow;

DECLARE @Templates table (
    RouteId int, Slot int, SourceTripId int, VehicleId int,
    DepartureUtc datetime2, ArrivalUtc datetime2, Fare decimal(18,2),
    DriverId nvarchar(450), CoDriverId nvarchar(450), StaffId nvarchar(450),
    PRIMARY KEY (RouteId,Slot)
);
INSERT @Templates
SELECT r.Id,slot.Number,t.Id,v.Id,t.DepartureUtc,t.ArrivalUtc,t.Fare,t.DriverId,t.CoDriverId,t.StaffId
FROM dbo.Routes r
CROSS JOIN (VALUES (1),(2)) slot(Number)
JOIN dbo.Vehicles v ON v.Plate=CONCAT(N'FLEET-',CASE WHEN r.Id<10 THEN N'0' ELSE N'' END,r.Id,N'-',slot.Number)
JOIN dbo.Layouts layout ON layout.Id=v.SeatLayoutId AND layout.IsActive=1
CROSS APPLY (
    SELECT TOP (1) t.Id,t.DepartureUtc,t.ArrivalUtc,t.Fare,t.DriverId,t.CoDriverId,t.StaffId
    FROM dbo.Trips t
    WHERE t.BusRouteId=r.Id AND t.VehicleId=v.Id AND t.Status=N'Scheduled' AND t.SalesOpen=1
    ORDER BY t.DepartureUtc DESC,t.Id DESC
) t
WHERE r.IsActive=1 AND v.IsActive=1;

IF NOT EXISTS (SELECT 1 FROM @Templates) OR EXISTS (
    SELECT 1 FROM dbo.Routes r
    LEFT JOIN @Templates first ON first.RouteId=r.Id AND first.Slot=1
    LEFT JOIN @Templates second ON second.RouteId=r.Id AND second.Slot=2
    WHERE r.IsActive=1 AND (first.SourceTripId IS NULL OR second.SourceTripId IS NULL
        OR DATEDIFF(minute,CONVERT(time,DATEADD(hour,7,first.DepartureUtc)),
                           CONVERT(time,DATEADD(hour,7,second.DepartureUtc)))<>60)
)
    THROW 50002,N'Cần hai chuyến mẫu cách nhau 60 phút trên mỗi tuyến đang hoạt động.',1;

DECLARE @Plan table (
    RouteId int, ServiceDate date, Slot int, SourceTripId int, VehicleId int,
    DepartureUtc datetime2, ArrivalUtc datetime2, Fare decimal(18,2),
    DriverId nvarchar(450), CoDriverId nvarchar(450), StaffId nvarchar(450),
    PRIMARY KEY (RouteId,ServiceDate,Slot)
);
WITH Dates AS (
    SELECT @StartDate AS ServiceDate WHERE @StartDate<=@EndDate
    UNION ALL
    SELECT DATEADD(day,1,ServiceDate) FROM Dates WHERE ServiceDate<@EndDate
)
INSERT @Plan
SELECT t.RouteId,d.ServiceDate,t.Slot,t.SourceTripId,t.VehicleId,
       DATEADD(day,shift.Days,t.DepartureUtc),DATEADD(day,shift.Days,t.ArrivalUtc),
       t.Fare,t.DriverId,t.CoDriverId,t.StaffId
FROM @Templates t
CROSS JOIN Dates d
CROSS APPLY (VALUES (
    DATEDIFF(day,CONVERT(date,DATEADD(hour,7,t.DepartureUtc)),d.ServiceDate),
    DATEADD(hour,-7,CONVERT(datetime2,d.ServiceDate))
)) shift(Days,DayStartUtc)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Trips existing
    WHERE existing.BusRouteId=t.RouteId AND existing.DepartureUtc>=shift.DayStartUtc
        AND existing.DepartureUtc<DATEADD(day,1,shift.DayStartUtc)
)
OPTION (MAXRECURSION 366);

IF EXISTS (
    SELECT 1 FROM @Plan p
    CROSS APPLY (VALUES (p.DriverId,N'Driver'),(p.CoDriverId,N'Driver'),(p.StaffId,N'Staff')) assigned(UserId,RoleName)
    WHERE (assigned.RoleName=N'Driver' OR assigned.UserId IS NOT NULL)
        AND NOT EXISTS (
            SELECT 1 FROM dbo.AspNetUsers u
            JOIN dbo.AspNetUserRoles ur ON ur.UserId=u.Id
            JOIN dbo.AspNetRoles r ON r.Id=ur.RoleId
            WHERE u.Id=assigned.UserId AND u.IsActive=1 AND r.Name=assigned.RoleName
        )
) OR EXISTS (SELECT 1 FROM @Plan WHERE DriverId=CoDriverId)
    THROW 50003,N'Phân công mẫu có tài khoản thiếu vai trò hoặc không hoạt động.',1;

IF EXISTS (
    SELECT 1 FROM @Plan p
    JOIN dbo.Trips t ON t.Status<>N'Cancelled'
        AND t.DepartureUtc<DATEADD(minute,@ScheduleBufferMinutes,p.ArrivalUtc)
        AND t.ArrivalUtc>DATEADD(minute,-@ScheduleBufferMinutes,p.DepartureUtc)
    WHERE t.VehicleId=p.VehicleId OR t.DriverId IN (p.DriverId,p.CoDriverId)
        OR t.CoDriverId IN (p.DriverId,p.CoDriverId) OR t.StaffId=p.StaffId
) OR EXISTS (
    SELECT 1 FROM @Plan p
    JOIN @Plan q ON (p.RouteId<>q.RouteId OR p.ServiceDate<>q.ServiceDate OR p.Slot<>q.Slot)
        AND q.DepartureUtc<DATEADD(minute,@ScheduleBufferMinutes,p.ArrivalUtc)
        AND q.ArrivalUtc>DATEADD(minute,-@ScheduleBufferMinutes,p.DepartureUtc)
    WHERE q.VehicleId=p.VehicleId OR q.DriverId IN (p.DriverId,p.CoDriverId)
        OR q.CoDriverId IN (p.DriverId,p.CoDriverId) OR q.StaffId=p.StaffId
)
    THROW 50004,N'Lịch mới bị trùng xe hoặc người được phân công.',1;

IF EXISTS (
    SELECT 1 FROM @Plan p
    JOIN dbo.Vehicles v ON v.Id=p.VehicleId
    WHERE NOT EXISTS (SELECT 1 FROM dbo.PhysicalSeats s WHERE s.SeatLayoutId=v.SeatLayoutId)
       OR NOT EXISTS (SELECT 1 FROM dbo.TripStops s WHERE s.TripId=p.SourceTripId AND s.IsActive=1
                      AND s.OffsetMinutes=0 AND s.CanPickup=1)
       OR NOT EXISTS (SELECT 1 FROM dbo.TripStops s WHERE s.TripId=p.SourceTripId AND s.IsActive=1
                      AND s.OffsetMinutes=DATEDIFF(minute,p.DepartureUtc,p.ArrivalUtc) AND s.CanDropoff=1)
)
    THROW 50005,N'Chuyến mẫu thiếu sơ đồ ghế hoặc điểm đầu/cuối hợp lệ.',1;

IF @Apply=1
BEGIN
    DECLARE @Written table (TripId int PRIMARY KEY, RouteId int, DepartureUtc datetime2);
    INSERT dbo.Trips(BusRouteId,VehicleId,DepartureUtc,ArrivalUtc,Fare,[Status],SalesOpen,Revision,DriverId,CoDriverId,StaffId)
    OUTPUT inserted.Id,inserted.BusRouteId,inserted.DepartureUtc INTO @Written
    SELECT RouteId,VehicleId,DepartureUtc,ArrivalUtc,Fare,N'Scheduled',1,1,DriverId,CoDriverId,StaffId
    FROM @Plan;

    INSERT dbo.TripSeats(TripId,PhysicalSeatId,Code,[Row],[Column],Floor,[Status],CurrentHoldId)
    SELECT w.TripId,s.Id,s.Code,s.[Row],s.[Column],s.Floor,N'Available',NULL
    FROM @Written w
    JOIN dbo.Trips t ON t.Id=w.TripId
    JOIN dbo.Vehicles v ON v.Id=t.VehicleId
    JOIN dbo.PhysicalSeats s ON s.SeatLayoutId=v.SeatLayoutId;

    INSERT dbo.TripStops(TripId,StopOrder,Name,Address,OffsetMinutes,CanPickup,CanDropoff,IsActive,IsIllustrative)
    SELECT w.TripId,s.StopOrder,s.Name,s.Address,s.OffsetMinutes,s.CanPickup,s.CanDropoff,1,s.IsIllustrative
    FROM @Written w
    JOIN @Plan p ON p.RouteId=w.RouteId AND p.DepartureUtc=w.DepartureUtc
    JOIN dbo.TripStops s ON s.TripId=p.SourceTripId AND s.IsActive=1;

    INSERT dbo.AuditEntries(ActorId,Action,EntityId,CreatedUtc,Detail)
    SELECT N'command',N'ScheduleExtended',CONVERT(nvarchar(100),w.TripId),@Now,
           CONCAT(N'Chuyến mẫu #',p.SourceTripId)
    FROM @Written w
    JOIN @Plan p ON p.RouteId=w.RouteId AND p.DepartureUtc=w.DepartureUtc;
END;

SELECT @Apply AS Applied,COUNT(*) AS NewTrips,COUNT(DISTINCT RouteId) AS Routes
FROM @Plan;
SELECT r.Origin,r.Destination,MIN(p.ServiceDate) AS FirstNewDay,MAX(p.ServiceDate) AS LastNewDay,COUNT(*) AS NewTrips
FROM @Plan p JOIN dbo.Routes r ON r.Id=p.RouteId
GROUP BY r.Id,r.Origin,r.Destination
ORDER BY r.Id;
COMMIT;
