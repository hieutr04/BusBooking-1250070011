SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId=N'20261105000200_AddDriverProfileAndVehicleType'
)
    THROW 50001,N'Áp dụng migration AddDriverProfileAndVehicleType trước.',1;

IF NOT EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId=N'20261106000100_AddSimulatedPayment'
)
BEGIN
    ALTER TABLE dbo.Bookings ADD SimulatedPaidUtc datetime2 NULL, SimulatedPaidAmount decimal(18,2) NULL;
    EXEC(N'ALTER TABLE dbo.Bookings ADD CONSTRAINT CK_Booking_SimulatedPayment CHECK (
        (SimulatedPaidUtc IS NULL AND SimulatedPaidAmount IS NULL)
        OR (SimulatedPaidUtc IS NOT NULL AND SimulatedPaidAmount IS NOT NULL AND SimulatedPaidAmount>0 AND SimulatedPaidAmount<=Total)
    )');
    INSERT dbo.__EFMigrationsHistory(MigrationId,ProductVersion)
    VALUES(N'20261106000100_AddSimulatedPayment',N'9.0.9');
END;

COMMIT;
