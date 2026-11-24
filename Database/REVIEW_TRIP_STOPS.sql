-- READ ONLY: run in SSMS against the intended database AFTER AddTripStops.
-- Shows real IDs/times; does not create/reset/update a trip or a booking.
SELECT t.Id AS TripId, r.Origin, r.Destination, t.VehicleId, t.Fare,
       DATEADD(hour,7,t.DepartureUtc) AS TripDepartureVN,
       DATEADD(hour,7,t.ArrivalUtc) AS TripArrivalVN,
       s.Id AS StopId,s.StopOrder,s.Name,s.Address,s.CanPickup,s.CanDropoff,s.IsActive,s.IsIllustrative,
       DATEADD(hour,7,CASE WHEN s.StopOrder=1000 THEN t.ArrivalUtc
           ELSE DATEADD(minute,s.OffsetMinutes,t.DepartureUtc) END) AS StopAtVN
FROM dbo.Trips t
JOIN dbo.Routes r ON r.Id=t.BusRouteId
LEFT JOIN dbo.TripStops s ON s.TripId=t.Id
ORDER BY t.Id,s.StopOrder;

SELECT TripId,COUNT(*) AS SeatCount FROM dbo.TripSeats GROUP BY TripId ORDER BY TripId;
SELECT MigrationId,ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;
