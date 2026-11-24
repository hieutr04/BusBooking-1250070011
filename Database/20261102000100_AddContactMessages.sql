-- AddContactMessages only. Companion deployment script for the EF migration.
-- Authored/reviewed against the migration; not emitted by dotnet ef in this environment.
-- Back up first. In SSMS SELECT THE EXISTING APPLICATION DATABASE in the toolbar.
-- Run as a database administrator. The runtime SQL login can stay read/write only.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() IN (N'master',N'model',N'msdb',N'tempdb')
    THROW 51100, N'Chon database ung dung hien co truoc khi chay script.', 1;
IF @@TRANCOUNT <> 0
    THROW 51101, N'Mo New Query khong co transaction dang chay.', 1;
IF ISNULL(HAS_PERMS_BY_NAME(DB_NAME(),N'DATABASE',N'CONTROL'),0) <> 1
    THROW 51102, N'Chay script bang tai khoan quan tri database.', 1;
IF OBJECT_ID(N'dbo.__EFMigrationsHistory',N'U') IS NULL
    THROW 51103, N'Thieu migration history. Dung lai; khong chay lai Initial hoac legacy schema.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @result int;
    EXEC @result=sys.sp_getapplock @Resource=N'BusBooking.ContactMigration',
        @LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=15000;
    IF @result < 0 THROW 51104, N'Migration dang duoc xu ly boi phien khac.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId=N'20261029000100_InitialSqlServer')
        THROW 51105, N'Chua ghi nhan Initial hien co. Dung lai de kiem tra; khong sua history bang script nay.', 1;
    IF EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId=N'20261102000100_AddContactMessages')
    BEGIN
        IF OBJECT_ID(N'dbo.ContactMessages',N'U') IS NULL
            THROW 51106, N'History da co migration nhung thieu bang ContactMessages. Dung lai de kiem tra.', 1;
        COMMIT TRANSACTION;
        SELECT N'ALREADY_APPLIED' AS Status, DB_NAME() AS DatabaseName;
        RETURN;
    END;
    IF OBJECT_ID(N'dbo.ContactMessages',N'U') IS NOT NULL
        THROW 51107, N'Bang ContactMessages da ton tai nhung chua co history tuong ung. Dung lai de doi chieu.', 1;

    CREATE TABLE [dbo].[ContactMessages] (
        [Id] uniqueidentifier NOT NULL,
        [SubmissionKey] uniqueidentifier NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [Email] nvarchar(254) NOT NULL,
        [Phone] nvarchar(25) NULL,
        [Subject] nvarchar(160) NOT NULL,
        [Body] nvarchar(4000) NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [IsRead] bit NOT NULL,
        CONSTRAINT [PK_ContactMessages] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_ContactMessages_SubmissionKey] ON [dbo].[ContactMessages] ([SubmissionKey]);
    CREATE INDEX [IX_ContactMessages_IsRead_CreatedUtc] ON [dbo].[ContactMessages] ([IsRead],[CreatedUtc]);
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId],[ProductVersion])
        VALUES (N'20261102000100_AddContactMessages',N'9.0.9');
    COMMIT TRANSACTION;
    SELECT N'CONTACT_MIGRATION_APPLIED' AS Status, DB_NAME() AS DatabaseName;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
