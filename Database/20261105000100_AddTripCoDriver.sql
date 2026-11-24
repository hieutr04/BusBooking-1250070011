SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId=N'20261104000100_AddTicketCheckIn'
)
    THROW 50001,N'Áp dụng migration AddTicketCheckIn trước.',1;

IF NOT EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId=N'20261105000100_AddTripCoDriver'
)
BEGIN
    ALTER TABLE dbo.Trips ADD CoDriverId nvarchar(450) NULL;
    EXEC(N'CREATE INDEX IX_Trips_CoDriverId_DepartureUtc_ArrivalUtc ON dbo.Trips(CoDriverId,DepartureUtc,ArrivalUtc)');
    EXEC(N'ALTER TABLE dbo.Trips ADD CONSTRAINT FK_Trips_AspNetUsers_CoDriverId FOREIGN KEY (CoDriverId) REFERENCES dbo.AspNetUsers(Id)');
    INSERT dbo.__EFMigrationsHistory(MigrationId,ProductVersion)
    VALUES(N'20261105000100_AddTripCoDriver',N'9.0.9');
END;

COMMIT;
