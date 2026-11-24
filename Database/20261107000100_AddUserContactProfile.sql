SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId=N'20261106000100_AddSimulatedPayment'
)
    THROW 50001,N'Áp dụng migration AddSimulatedPayment trước.',1;

IF NOT EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId=N'20261107000100_AddUserContactProfile'
)
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD Address nvarchar(300) NULL, ProvinceCode nvarchar(2) NULL;
    ALTER TABLE dbo.AspNetUsers ADD CreatedUtc datetime2 NOT NULL
        CONSTRAINT DF_AspNetUsers_CreatedUtc DEFAULT SYSUTCDATETIME();
    INSERT dbo.__EFMigrationsHistory(MigrationId,ProductVersion)
    VALUES(N'20261107000100_AddUserContactProfile',N'9.0.9');
END;
COMMIT;
