-- Hồ sơ giả lập cho đồ án. Các tài khoản này chưa được cấp mật khẩu đăng nhập.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId=N'20261105000200_AddDriverProfileAndVehicleType'
)
    THROW 50001,N'Áp dụng migration AddDriverProfileAndVehicleType trước.',1;

DECLARE @LockResult int;
EXEC @LockResult=sp_getapplock @Resource=N'BusBooking.Catalog',
    @LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000;
IF @LockResult<0
    THROW 50002,N'Danh mục đang được cập nhật. Hãy thử lại.',1;

DECLARE @DriverRoleId nvarchar(450)=(
    SELECT Id FROM dbo.AspNetRoles WHERE NormalizedName=N'DRIVER'
);
IF @DriverRoleId IS NULL
    THROW 50003,N'Cần khởi tạo vai trò tài xế trước.',1;

DECLARE @Profiles table (Number int PRIMARY KEY, FullName nvarchar(100), DateOfBirth date);
INSERT @Profiles(Number,FullName,DateOfBirth) VALUES
    (1,N'Nguyễn Minh Khang','19830314'),
    (2,N'Trần Quốc Huy','19870925'),
    (3,N'Lê Thành Đạt','19900517'),
    (4,N'Phạm Hoàng Nam','19850611'),
    (5,N'Võ Anh Tuấn','19920208'),
    (6,N'Đặng Đức Thành','19791121'),
    (7,N'Bùi Nhật Minh','19940719'),
    (8,N'Đỗ Văn Phúc','19810903'),
    (9,N'Hồ Quang Vinh','19881214'),
    (10,N'Phan Hữu Lộc','19770426'),
    (11,N'Vũ Trung Kiên','19860216'),
    (12,N'Ngô Đình Khánh','19910622'),
    (13,N'Dương Hải Long','19801005'),
    (14,N'Hoàng Quốc Bảo','19950318'),
    (15,N'Lý Văn Trường','19820727'),
    (16,N'Mai Thanh Tùng','19900916'),
    (17,N'Đinh Công Hậu','19841130'),
    (18,N'Tạ Minh Duy','19960112'),
    (19,N'Cao Văn Lâm','19761024'),
    (20,N'Trương Thế Anh','19890620'),
    (21,N'Nguyễn Bảo Sơn','19870403'),
    (22,N'Trần Hồng Quân','19810119'),
    (23,N'Lê Quang Hải','19930705'),
    (24,N'Phạm Đức Hưng','19780908'),
    (25,N'Võ Tuấn Anh','19911203'),
    (26,N'Đặng Hoài Phong','19940524'),
    (27,N'Bùi Thành Công','19851211'),
    (28,N'Đỗ Quốc Việt','19820307'),
    (29,N'Hồ Minh Trí','19970828'),
    (30,N'Phan Nhật Hào','19920920'),
    (31,N'Vũ Thị Thanh Tâm','19851107'),
    (32,N'Ngô Bảo Ngân','19930815'),
    (33,N'Dương Thị Thu Hà','19890219'),
    (34,N'Hoàng Minh Tân','19960609'),
    (35,N'Lý Văn Toàn','19780414'),
    (36,N'Mai Quốc Thịnh','19880805'),
    (37,N'Đinh Thành Luân','19951119'),
    (38,N'Tạ Đức Dũng','19841007'),
    (39,N'Cao Minh Hiếu','19940224'),
    (40,N'Trương Văn Nghĩa','19860728');

DECLARE @Accounts table (
    Id nvarchar(450) PRIMARY KEY, FullName nvarchar(100), DateOfBirth date,
    Email nvarchar(256), PhoneNumber nvarchar(25)
);
INSERT @Accounts(Id,FullName,DateOfBirth,Email,PhoneNumber)
SELECT CONCAT('f3aad073-591b-450f-a711-',RIGHT(CONCAT('000000000000',Number),12)),
       FullName,DateOfBirth,
       CONCAT('driver-demo-',RIGHT(CONCAT('00',Number),2),'@busbooking.test'),
       RIGHT(CONCAT('0000000000',Number),10)
FROM @Profiles;

IF EXISTS (
    SELECT 1 FROM @Accounts a
    INNER JOIN dbo.AspNetUsers u ON u.NormalizedEmail=UPPER(a.Email) OR u.NormalizedUserName=UPPER(a.Email)
    WHERE u.Id<>a.Id
)
    THROW 50004,N'Email tài xế giả lập trùng với một tài khoản khác.',1;

DECLARE @Added table (Id nvarchar(450) PRIMARY KEY);
INSERT dbo.AspNetUsers (
    Id,FullName,DateOfBirth,IsActive,UserName,NormalizedUserName,Email,NormalizedEmail,
    EmailConfirmed,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,
    TwoFactorEnabled,LockoutEnabled,AccessFailedCount
)
OUTPUT inserted.Id INTO @Added(Id)
SELECT a.Id,a.FullName,a.DateOfBirth,1,a.Email,UPPER(a.Email),a.Email,UPPER(a.Email),
       0,NEWID(),NEWID(),a.PhoneNumber,0,0,1,0
FROM @Accounts a
WHERE NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers u WHERE u.Id=a.Id);

INSERT dbo.AspNetUserRoles(UserId,RoleId)
SELECT Id,@DriverRoleId FROM @Added;

COMMIT;
SELECT COUNT(*) AS AddedDrivers FROM @Added;
