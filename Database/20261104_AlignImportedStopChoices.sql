SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @suffix nvarchar(30)=N' · Mẫu đồ án';
DECLARE @trips TABLE(Id int PRIMARY KEY, Duration int);
DECLARE @changes TABLE(TripId int, StopId int, IsActive bit);

-- Take the same trip locks as the booking service before changing selectable points.
INSERT INTO @trips(Id,Duration)
SELECT t.Id,DATEDIFF(minute,t.DepartureUtc,t.ArrivalUtc)
FROM dbo.Trips t WITH (UPDLOCK, ROWLOCK)
JOIN dbo.Routes r ON r.Id=t.BusRouteId
JOIN dbo.Vehicles v ON v.Id=t.VehicleId
WHERE t.Status='Scheduled' AND t.SalesOpen=1 AND t.DepartureUtc>SYSUTCDATETIME()
AND v.Plate IN ('TEST-001','TEST-002','TEST-003')
AND (
    (r.Origin=N'TP. Hồ Chí Minh' AND r.Destination IN (N'Đà Lạt',N'Nha Trang'))
    OR (r.Origin=N'Đà Lạt' AND r.Destination=N'TP. Hồ Chí Minh')
);

UPDATE s
SET Name=LEFT(s.Name,LEN(s.Name)-LEN(@suffix))
OUTPUT inserted.TripId,inserted.Id,inserted.IsActive INTO @changes
FROM dbo.TripStops s JOIN @trips t ON t.Id=s.TripId
WHERE s.IsActive=1 AND RIGHT(s.Name,LEN(@suffix))=@suffix
AND ((s.OffsetMinutes=0 AND s.CanPickup=1 AND s.CanDropoff=0)
  OR (s.OffsetMinutes=t.Duration AND s.CanPickup=0 AND s.CanDropoff=1));

UPDATE s
SET IsActive=0
OUTPUT inserted.TripId,inserted.Id,inserted.IsActive INTO @changes
FROM dbo.TripStops s JOIN @trips t ON t.Id=s.TripId
WHERE s.IsActive=1 AND RIGHT(s.Name,LEN(@suffix))=@suffix
AND s.OffsetMinutes>0 AND s.OffsetMinutes<t.Duration
AND NOT EXISTS (
    SELECT 1 FROM dbo.SeatHolds h
    WHERE h.TripId=t.Id AND h.Status='Active' AND h.ExpiresAtUtc>SYSUTCDATETIME()
    AND (h.PickupStopId=s.Id OR h.DropoffStopId=s.Id)
);

UPDATE t SET Revision=Revision+1
FROM dbo.Trips t WHERE EXISTS (SELECT 1 FROM @changes c WHERE c.TripId=t.Id);

INSERT dbo.AuditEntries (CreatedUtc,ActorId,Action,EntityId,Detail)
SELECT SYSUTCDATETIME(),N'command',N'ImportedStopAligned',CONVERT(nvarchar(100),StopId),
       CASE WHEN IsActive=1 THEN N'Normalize terminal name' ELSE N'Retire imported intermediate choice' END
FROM @changes;

COMMIT;

SELECT COUNT(DISTINCT TripId) AS UpdatedTrips,
       COUNT(CASE WHEN IsActive=1 THEN 1 END) AS RenamedTerminals,
       COUNT(CASE WHEN IsActive=0 THEN 1 END) AS RetiredIntermediateStops
FROM @changes;
