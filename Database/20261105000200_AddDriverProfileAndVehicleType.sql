SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId=N'20261105000100_AddTripCoDriver'
)
    THROW 50001,N'Áp dụng migration AddTripCoDriver trước.',1;

IF NOT EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId=N'20261105000200_AddDriverProfileAndVehicleType'
)
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD DateOfBirth date NULL;
    ALTER TABLE dbo.Vehicles ADD Kind nvarchar(12) NULL;
    EXEC(N'UPDATE v SET Kind=CASE WHEN l.Floors>1 OR l.Name LIKE N''%giường nằm%''
                                 THEN N''Sleeper'' ELSE N''Seated'' END
           FROM dbo.Vehicles v INNER JOIN dbo.Layouts l ON l.Id=v.SeatLayoutId');
    EXEC(N'ALTER TABLE dbo.Vehicles ALTER COLUMN Kind nvarchar(12) NOT NULL;
           ALTER TABLE dbo.Vehicles ADD CONSTRAINT CK_Vehicle_Kind CHECK (Kind IN (''Seated'',''Sleeper''));');

    UPDATE u SET FullName=CASE u.NormalizedEmail
        WHEN N'FLEET-DRIVER-01@BUSBOOKING.TEST' THEN N'Nguyễn Văn A'
        WHEN N'FLEET-DRIVER-02@BUSBOOKING.TEST' THEN N'Trần Bảo A' END
    FROM dbo.AspNetUsers u
    INNER JOIN dbo.AspNetUserRoles ur ON ur.UserId=u.Id
    INNER JOIN dbo.AspNetRoles r ON r.Id=ur.RoleId AND r.NormalizedName=N'DRIVER'
    WHERE (u.NormalizedEmail=N'FLEET-DRIVER-01@BUSBOOKING.TEST' AND u.FullName=N'Tài xế đội xe 01')
       OR (u.NormalizedEmail=N'FLEET-DRIVER-02@BUSBOOKING.TEST' AND u.FullName=N'Tài xế đội xe 02');

    INSERT dbo.__EFMigrationsHistory(MigrationId,ProductVersion)
    VALUES(N'20261105000200_AddDriverProfileAndVehicleType',N'9.0.9');
END;

COMMIT;
