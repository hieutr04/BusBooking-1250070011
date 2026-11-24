SET XACT_ABORT ON;

IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103000100_AddTripStops'
)
    THROW 50001, N'Apply AddTripStops before AddTicketCheckIn.', 1;

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104000100_AddTicketCheckIn'
)
BEGIN
    ALTER TABLE [Tickets] ADD [CheckedInUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104000100_AddTicketCheckIn'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104000100_AddTicketCheckIn', N'9.0.9');
END;

COMMIT;
GO

