using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace BusBooking.Data.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("20261029000100_InitialSqlServer")]
public class InitialSqlServer:Migration {
 protected override void Up(MigrationBuilder migrationBuilder) {
 migrationBuilder.Sql("""
CREATE TABLE [dbo].[AspNetUsers] (
 [Id] nvarchar(450) NOT NULL,
 [FullName] nvarchar(100) NOT NULL,
 [IsActive] bit NOT NULL,
 [UserName] nvarchar(256) NULL,
 [NormalizedUserName] nvarchar(256) NULL,
 [Email] nvarchar(256) NULL,
 [NormalizedEmail] nvarchar(256) NULL,
 [EmailConfirmed] bit NOT NULL,
 [PasswordHash] nvarchar(max) NULL,
 [SecurityStamp] nvarchar(max) NULL,
 [ConcurrencyStamp] nvarchar(max) NULL,
 [PhoneNumber] nvarchar(max) NULL,
 [PhoneNumberConfirmed] bit NOT NULL,
 [TwoFactorEnabled] bit NOT NULL,
 [LockoutEnd] datetimeoffset NULL,
 [LockoutEnabled] bit NOT NULL,
 [AccessFailedCount] int NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
CREATE TABLE [dbo].[AspNetRoles] (
 [Id] nvarchar(450) NOT NULL,
 [Name] nvarchar(256) NULL,
 [NormalizedName] nvarchar(256) NULL,
 [ConcurrencyStamp] nvarchar(max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
CREATE TABLE [dbo].[AspNetUserRoles] (
 [UserId] nvarchar(450) NOT NULL,
 [RoleId] nvarchar(450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId])
);
CREATE TABLE [dbo].[AspNetUserClaims] (
 [Id] int IDENTITY(1,1) NOT NULL,
 [UserId] nvarchar(450) NOT NULL,
 [ClaimType] nvarchar(max) NULL,
 [ClaimValue] nvarchar(max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id])
);
CREATE TABLE [dbo].[AspNetRoleClaims] (
 [Id] int IDENTITY(1,1) NOT NULL,
 [RoleId] nvarchar(450) NOT NULL,
 [ClaimType] nvarchar(max) NULL,
 [ClaimValue] nvarchar(max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id])
);
CREATE TABLE [dbo].[AspNetUserLogins] (
 [LoginProvider] nvarchar(128) NOT NULL,
 [ProviderKey] nvarchar(128) NOT NULL,
 [ProviderDisplayName] nvarchar(max) NULL,
 [UserId] nvarchar(450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey])
);
CREATE TABLE [dbo].[AspNetUserTokens] (
 [UserId] nvarchar(450) NOT NULL,
 [LoginProvider] nvarchar(128) NOT NULL,
 [Name] nvarchar(128) NOT NULL,
 [Value] nvarchar(max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name])
);
CREATE TABLE [dbo].[Routes] (
 [Id] int IDENTITY(1,1) NOT NULL,
 [Origin] nvarchar(100) NOT NULL,
 [Destination] nvarchar(100) NOT NULL,
 [Description] nvarchar(500) NOT NULL,
 [DurationMinutes] int NOT NULL,
 [IsActive] bit NOT NULL,
 CONSTRAINT [PK_Routes] PRIMARY KEY ([Id]),
 CONSTRAINT [CK_Route_Duration] CHECK ([DurationMinutes] > 0 AND [Origin] <> [Destination])
);
CREATE TABLE [dbo].[Layouts] (
 [Id] int IDENTITY(1,1) NOT NULL,
 [Name] nvarchar(100) NOT NULL,
 [Rows] int NOT NULL,
 [Columns] int NOT NULL,
 [Floors] int NOT NULL,
 [IsActive] bit NOT NULL,
 CONSTRAINT [PK_Layouts] PRIMARY KEY ([Id]),
 CONSTRAINT [CK_Layout_Shape] CHECK ([Rows] BETWEEN 1 AND 20 AND [Columns] BETWEEN 1 AND 4 AND [Floors] BETWEEN 1 AND 2)
);
CREATE TABLE [dbo].[PhysicalSeats] (
 [Id] int IDENTITY(1,1) NOT NULL,
 [SeatLayoutId] int NOT NULL,
 [Code] nvarchar(10) NOT NULL,
 [Row] int NOT NULL,
 [Column] int NOT NULL,
 [Floor] int NOT NULL,
 CONSTRAINT [PK_PhysicalSeats] PRIMARY KEY ([Id]),
 CONSTRAINT [CK_PhysicalSeat_Position] CHECK ([Row]>0 AND [Column]>0 AND [Floor]>0)
);
CREATE TABLE [dbo].[Vehicles] (
 [Id] int IDENTITY(1,1) NOT NULL,
 [Plate] nvarchar(30) NOT NULL,
 [Name] nvarchar(100) NOT NULL,
 [SeatLayoutId] int NOT NULL,
 [IsActive] bit NOT NULL,
 CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id])
);
CREATE TABLE [dbo].[Trips] (
 [Id] int IDENTITY(1,1) NOT NULL,
 [BusRouteId] int NOT NULL,
 [VehicleId] int NOT NULL,
 [DepartureUtc] datetime2 NOT NULL,
 [ArrivalUtc] datetime2 NOT NULL,
 [Fare] decimal(18,2) NOT NULL,
 [Status] nvarchar(20) NOT NULL,
 [SalesOpen] bit NOT NULL,
 [Revision] bigint NOT NULL,
 [DriverId] nvarchar(450) NULL,
 [StaffId] nvarchar(450) NULL,
 CONSTRAINT [PK_Trips] PRIMARY KEY ([Id]),
 CONSTRAINT [CK_Trip_Values] CHECK ([ArrivalUtc]>[DepartureUtc] AND [Fare]>=0 AND [Revision]>=0 AND [Status] IN ('Scheduled','Boarding','InProgress','Completed','Cancelled'))
);
CREATE TABLE [dbo].[TripSeats] (
 [Id] int IDENTITY(1,1) NOT NULL,
 [TripId] int NOT NULL,
 [PhysicalSeatId] int NOT NULL,
 [Code] nvarchar(10) NOT NULL,
 [Row] int NOT NULL,
 [Column] int NOT NULL,
 [Floor] int NOT NULL,
 [Status] nvarchar(12) NOT NULL,
 [CurrentHoldId] uniqueidentifier NULL,
 CONSTRAINT [PK_TripSeats] PRIMARY KEY ([Id]),
 CONSTRAINT [AK_TripSeats_Id_TripId] UNIQUE ([Id], [TripId]),
 CONSTRAINT [CK_TripSeat_State] CHECK (([Status]='Held' AND [CurrentHoldId] IS NOT NULL) OR ([Status] IN ('Available','Booked') AND [CurrentHoldId] IS NULL))
);
CREATE TABLE [dbo].[SeatHolds] (
 [Id] uniqueidentifier NOT NULL,
 [TripId] int NOT NULL,
 [OwnerId] nvarchar(450) NOT NULL,
 [RequestKey] uniqueidentifier NOT NULL,
 [CreatedUtc] datetime2 NOT NULL,
 [ExpiresAtUtc] datetime2 NOT NULL,
 [Status] nvarchar(12) NOT NULL,
 CONSTRAINT [PK_SeatHolds] PRIMARY KEY ([Id]),
 CONSTRAINT [AK_SeatHolds_Id_TripId] UNIQUE ([Id], [TripId]),
 CONSTRAINT [CK_Hold_Expiry] CHECK ([ExpiresAtUtc]>[CreatedUtc] AND [Status] IN ('Active','Released','Expired','Confirmed'))
);
CREATE TABLE [dbo].[HoldSeats] (
 [SeatHoldId] uniqueidentifier NOT NULL,
 [TripSeatId] int NOT NULL,
 [TripId] int NOT NULL,
 CONSTRAINT [PK_HoldSeats] PRIMARY KEY ([SeatHoldId], [TripSeatId])
);
CREATE TABLE [dbo].[Bookings] (
 [Id] uniqueidentifier NOT NULL,
 [SeatHoldId] uniqueidentifier NOT NULL,
 [Code] nvarchar(30) NOT NULL,
 [OwnerId] nvarchar(450) NOT NULL,
 [ContactName] nvarchar(100) NOT NULL,
 [ContactPhone] nvarchar(25) NOT NULL,
 [CreatedUtc] datetime2 NOT NULL,
 [Status] nvarchar(12) NOT NULL,
 [Total] decimal(18,2) NOT NULL,
 [ReplacesBookingId] uniqueidentifier NULL,
 CONSTRAINT [PK_Bookings] PRIMARY KEY ([Id]),
 CONSTRAINT [CK_Booking_Total] CHECK ([Total]>=0 AND [Status] IN ('Active','Cancelled','Exchanged'))
);
CREATE TABLE [dbo].[Tickets] (
 [Id] uniqueidentifier NOT NULL,
 [BookingId] uniqueidentifier NOT NULL,
 [TripSeatId] int NOT NULL,
 [PassengerName] nvarchar(100) NOT NULL,
 [Fare] decimal(18,2) NOT NULL,
 [IsActive] bit NOT NULL,
 CONSTRAINT [PK_Tickets] PRIMARY KEY ([Id]),
 CONSTRAINT [CK_Ticket_Fare] CHECK ([Fare]>=0)
);
CREATE TABLE [dbo].[Payments] (
 [Id] uniqueidentifier NOT NULL,
 [BookingId] uniqueidentifier NOT NULL,
 [Kind] nvarchar(12) NOT NULL,
 [Amount] decimal(18,2) NOT NULL,
 [ActorId] nvarchar(450) NOT NULL,
 [RequestKey] uniqueidentifier NOT NULL,
 [CreatedUtc] datetime2 NOT NULL,
 [Note] nvarchar(300) NOT NULL,
 CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
 CONSTRAINT [CK_Payment_Sign] CHECK (([Kind]='Receipt' AND [Amount]>0) OR ([Kind]='Refund' AND [Amount]<0))
);
CREATE TABLE [dbo].[ChangeRequests] (
 [Id] uniqueidentifier NOT NULL,
 [BookingId] uniqueidentifier NOT NULL,
 [Kind] nvarchar(12) NOT NULL,
 [Status] nvarchar(12) NOT NULL,
 [Reason] nvarchar(500) NOT NULL,
 [Resolution] nvarchar(500) NOT NULL,
 [CreatedUtc] datetime2 NOT NULL,
 [ResolvedUtc] datetime2 NULL,
 [ResolvedBy] nvarchar(450) NULL,
 CONSTRAINT [PK_ChangeRequests] PRIMARY KEY ([Id])
);
CREATE TABLE [dbo].[AuditEntries] (
 [Id] bigint IDENTITY(1,1) NOT NULL,
 [CreatedUtc] datetime2 NOT NULL,
 [ActorId] nvarchar(450) NOT NULL,
 [Action] nvarchar(60) NOT NULL,
 [EntityId] nvarchar(100) NOT NULL,
 [Detail] nvarchar(500) NOT NULL,
 CONSTRAINT [PK_AuditEntries] PRIMARY KEY ([Id])
);
CREATE INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail]);
CREATE UNIQUE INDEX [UserNameIndex] ON [dbo].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
CREATE UNIQUE INDEX [RoleNameIndex] ON [dbo].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
ALTER TABLE [dbo].[AspNetUserRoles] ADD CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE NO ACTION;
ALTER TABLE [dbo].[AspNetUserRoles] ADD CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;
CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles] ([RoleId]);
ALTER TABLE [dbo].[AspNetUserClaims] ADD CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;
CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims] ([UserId]);
ALTER TABLE [dbo].[AspNetRoleClaims] ADD CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE NO ACTION;
CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims] ([RoleId]);
ALTER TABLE [dbo].[AspNetUserLogins] ADD CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins] ([UserId]);
ALTER TABLE [dbo].[AspNetUserTokens] ADD CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;
CREATE UNIQUE INDEX [IX_Routes_Origin_Destination] ON [dbo].[Routes] ([Origin], [Destination]);
ALTER TABLE [dbo].[PhysicalSeats] ADD CONSTRAINT [FK_PhysicalSeats_Layouts_SeatLayoutId] FOREIGN KEY ([SeatLayoutId]) REFERENCES [dbo].[Layouts] ([Id]) ON DELETE NO ACTION;
CREATE UNIQUE INDEX [IX_PhysicalSeats_SeatLayoutId_Code] ON [dbo].[PhysicalSeats] ([SeatLayoutId], [Code]);
CREATE UNIQUE INDEX [IX_PhysicalSeats_SeatLayoutId_Floor_Row_Column] ON [dbo].[PhysicalSeats] ([SeatLayoutId], [Floor], [Row], [Column]);
ALTER TABLE [dbo].[Vehicles] ADD CONSTRAINT [FK_Vehicles_Layouts_SeatLayoutId] FOREIGN KEY ([SeatLayoutId]) REFERENCES [dbo].[Layouts] ([Id]) ON DELETE NO ACTION;
CREATE UNIQUE INDEX [IX_Vehicles_Plate] ON [dbo].[Vehicles] ([Plate]);
CREATE INDEX [IX_Vehicles_SeatLayoutId] ON [dbo].[Vehicles] ([SeatLayoutId]);
ALTER TABLE [dbo].[Trips] ADD CONSTRAINT [FK_Trips_Routes_BusRouteId] FOREIGN KEY ([BusRouteId]) REFERENCES [dbo].[Routes] ([Id]) ON DELETE NO ACTION;
ALTER TABLE [dbo].[Trips] ADD CONSTRAINT [FK_Trips_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [dbo].[Vehicles] ([Id]) ON DELETE NO ACTION;
ALTER TABLE [dbo].[Trips] ADD CONSTRAINT [FK_Trips_AspNetUsers_DriverId] FOREIGN KEY ([DriverId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;
ALTER TABLE [dbo].[Trips] ADD CONSTRAINT [FK_Trips_AspNetUsers_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;
CREATE INDEX [IX_Trips_BusRouteId_DepartureUtc_Status_SalesOpen] ON [dbo].[Trips] ([BusRouteId], [DepartureUtc], [Status], [SalesOpen]);
CREATE INDEX [IX_Trips_VehicleId_DepartureUtc_ArrivalUtc] ON [dbo].[Trips] ([VehicleId], [DepartureUtc], [ArrivalUtc]);
CREATE INDEX [IX_Trips_DriverId_DepartureUtc_ArrivalUtc] ON [dbo].[Trips] ([DriverId], [DepartureUtc], [ArrivalUtc]);
CREATE INDEX [IX_Trips_StaffId] ON [dbo].[Trips] ([StaffId]);
ALTER TABLE [dbo].[TripSeats] ADD CONSTRAINT [FK_TripSeats_Trips_TripId] FOREIGN KEY ([TripId]) REFERENCES [dbo].[Trips] ([Id]) ON DELETE NO ACTION;
ALTER TABLE [dbo].[TripSeats] ADD CONSTRAINT [FK_TripSeats_PhysicalSeats_PhysicalSeatId] FOREIGN KEY ([PhysicalSeatId]) REFERENCES [dbo].[PhysicalSeats] ([Id]) ON DELETE NO ACTION;
ALTER TABLE [dbo].[TripSeats] ADD CONSTRAINT [FK_TripSeats_SeatHolds_CurrentHoldId_TripId] FOREIGN KEY ([CurrentHoldId], [TripId]) REFERENCES [dbo].[SeatHolds] ([Id], [TripId]) ON DELETE NO ACTION;
CREATE UNIQUE INDEX [IX_TripSeats_TripId_PhysicalSeatId] ON [dbo].[TripSeats] ([TripId], [PhysicalSeatId]);
CREATE UNIQUE INDEX [IX_TripSeats_TripId_Code] ON [dbo].[TripSeats] ([TripId], [Code]);
CREATE INDEX [IX_TripSeats_TripId_Status] ON [dbo].[TripSeats] ([TripId], [Status]);
CREATE INDEX [IX_TripSeats_PhysicalSeatId] ON [dbo].[TripSeats] ([PhysicalSeatId]);
CREATE INDEX [IX_TripSeats_CurrentHoldId_TripId] ON [dbo].[TripSeats] ([CurrentHoldId], [TripId]);
ALTER TABLE [dbo].[SeatHolds] ADD CONSTRAINT [FK_SeatHolds_Trips_TripId] FOREIGN KEY ([TripId]) REFERENCES [dbo].[Trips] ([Id]) ON DELETE NO ACTION;
ALTER TABLE [dbo].[SeatHolds] ADD CONSTRAINT [FK_SeatHolds_AspNetUsers_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;
CREATE INDEX [IX_SeatHolds_TripId] ON [dbo].[SeatHolds] ([TripId]);
CREATE UNIQUE INDEX [IX_SeatHolds_OwnerId_RequestKey] ON [dbo].[SeatHolds] ([OwnerId], [RequestKey]);
CREATE INDEX [IX_SeatHolds_Status_ExpiresAtUtc_TripId] ON [dbo].[SeatHolds] ([Status], [ExpiresAtUtc], [TripId]);
ALTER TABLE [dbo].[HoldSeats] ADD CONSTRAINT [FK_HoldSeats_SeatHolds_SeatHoldId_TripId] FOREIGN KEY ([SeatHoldId], [TripId]) REFERENCES [dbo].[SeatHolds] ([Id], [TripId]) ON DELETE NO ACTION;
ALTER TABLE [dbo].[HoldSeats] ADD CONSTRAINT [FK_HoldSeats_TripSeats_TripSeatId_TripId] FOREIGN KEY ([TripSeatId], [TripId]) REFERENCES [dbo].[TripSeats] ([Id], [TripId]) ON DELETE NO ACTION;
CREATE INDEX [IX_HoldSeats_SeatHoldId_TripId] ON [dbo].[HoldSeats] ([SeatHoldId], [TripId]);
CREATE INDEX [IX_HoldSeats_TripSeatId_TripId] ON [dbo].[HoldSeats] ([TripSeatId], [TripId]);
ALTER TABLE [dbo].[Bookings] ADD CONSTRAINT [FK_Bookings_SeatHolds_SeatHoldId] FOREIGN KEY ([SeatHoldId]) REFERENCES [dbo].[SeatHolds] ([Id]) ON DELETE NO ACTION;
ALTER TABLE [dbo].[Bookings] ADD CONSTRAINT [FK_Bookings_AspNetUsers_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;
ALTER TABLE [dbo].[Bookings] ADD CONSTRAINT [FK_Bookings_Bookings_ReplacesBookingId] FOREIGN KEY ([ReplacesBookingId]) REFERENCES [dbo].[Bookings] ([Id]) ON DELETE NO ACTION;
CREATE UNIQUE INDEX [IX_Bookings_SeatHoldId] ON [dbo].[Bookings] ([SeatHoldId]);
CREATE UNIQUE INDEX [IX_Bookings_Code] ON [dbo].[Bookings] ([Code]);
CREATE INDEX [IX_Bookings_OwnerId_CreatedUtc] ON [dbo].[Bookings] ([OwnerId], [CreatedUtc]);
CREATE INDEX [IX_Bookings_Status_CreatedUtc] ON [dbo].[Bookings] ([Status], [CreatedUtc]);
CREATE INDEX [IX_Bookings_ReplacesBookingId] ON [dbo].[Bookings] ([ReplacesBookingId]);
ALTER TABLE [dbo].[Tickets] ADD CONSTRAINT [FK_Tickets_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [dbo].[Bookings] ([Id]) ON DELETE NO ACTION;
ALTER TABLE [dbo].[Tickets] ADD CONSTRAINT [FK_Tickets_TripSeats_TripSeatId] FOREIGN KEY ([TripSeatId]) REFERENCES [dbo].[TripSeats] ([Id]) ON DELETE NO ACTION;
CREATE INDEX [IX_Tickets_BookingId] ON [dbo].[Tickets] ([BookingId]);
CREATE UNIQUE INDEX [IX_Tickets_TripSeatId] ON [dbo].[Tickets] ([TripSeatId]) WHERE [IsActive] = 1;
ALTER TABLE [dbo].[Payments] ADD CONSTRAINT [FK_Payments_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [dbo].[Bookings] ([Id]) ON DELETE NO ACTION;
CREATE INDEX [IX_Payments_BookingId] ON [dbo].[Payments] ([BookingId]);
CREATE UNIQUE INDEX [IX_Payments_ActorId_RequestKey] ON [dbo].[Payments] ([ActorId], [RequestKey]);
CREATE INDEX [IX_Payments_CreatedUtc] ON [dbo].[Payments] ([CreatedUtc]);
ALTER TABLE [dbo].[ChangeRequests] ADD CONSTRAINT [FK_ChangeRequests_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [dbo].[Bookings] ([Id]) ON DELETE NO ACTION;
CREATE UNIQUE INDEX [IX_ChangeRequests_BookingId] ON [dbo].[ChangeRequests] ([BookingId]) WHERE [Status] = 'Pending';
CREATE INDEX [IX_AuditEntries_Action_CreatedUtc] ON [dbo].[AuditEntries] ([Action], [CreatedUtc]);
""");
 }
 protected override void Down(MigrationBuilder migrationBuilder) {
 migrationBuilder.Sql("""
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId];
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId];
ALTER TABLE [dbo].[AspNetUserClaims] DROP CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId];
ALTER TABLE [dbo].[AspNetRoleClaims] DROP CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId];
ALTER TABLE [dbo].[AspNetUserLogins] DROP CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId];
ALTER TABLE [dbo].[AspNetUserTokens] DROP CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId];
ALTER TABLE [dbo].[PhysicalSeats] DROP CONSTRAINT [FK_PhysicalSeats_Layouts_SeatLayoutId];
ALTER TABLE [dbo].[Vehicles] DROP CONSTRAINT [FK_Vehicles_Layouts_SeatLayoutId];
ALTER TABLE [dbo].[Trips] DROP CONSTRAINT [FK_Trips_Routes_BusRouteId];
ALTER TABLE [dbo].[Trips] DROP CONSTRAINT [FK_Trips_Vehicles_VehicleId];
ALTER TABLE [dbo].[Trips] DROP CONSTRAINT [FK_Trips_AspNetUsers_DriverId];
ALTER TABLE [dbo].[Trips] DROP CONSTRAINT [FK_Trips_AspNetUsers_StaffId];
ALTER TABLE [dbo].[TripSeats] DROP CONSTRAINT [FK_TripSeats_Trips_TripId];
ALTER TABLE [dbo].[TripSeats] DROP CONSTRAINT [FK_TripSeats_PhysicalSeats_PhysicalSeatId];
ALTER TABLE [dbo].[TripSeats] DROP CONSTRAINT [FK_TripSeats_SeatHolds_CurrentHoldId_TripId];
ALTER TABLE [dbo].[SeatHolds] DROP CONSTRAINT [FK_SeatHolds_Trips_TripId];
ALTER TABLE [dbo].[SeatHolds] DROP CONSTRAINT [FK_SeatHolds_AspNetUsers_OwnerId];
ALTER TABLE [dbo].[HoldSeats] DROP CONSTRAINT [FK_HoldSeats_SeatHolds_SeatHoldId_TripId];
ALTER TABLE [dbo].[HoldSeats] DROP CONSTRAINT [FK_HoldSeats_TripSeats_TripSeatId_TripId];
ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [FK_Bookings_SeatHolds_SeatHoldId];
ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [FK_Bookings_AspNetUsers_OwnerId];
ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [FK_Bookings_Bookings_ReplacesBookingId];
ALTER TABLE [dbo].[Tickets] DROP CONSTRAINT [FK_Tickets_Bookings_BookingId];
ALTER TABLE [dbo].[Tickets] DROP CONSTRAINT [FK_Tickets_TripSeats_TripSeatId];
ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK_Payments_Bookings_BookingId];
ALTER TABLE [dbo].[ChangeRequests] DROP CONSTRAINT [FK_ChangeRequests_Bookings_BookingId];
DROP TABLE [dbo].[AuditEntries];
DROP TABLE [dbo].[ChangeRequests];
DROP TABLE [dbo].[Payments];
DROP TABLE [dbo].[Tickets];
DROP TABLE [dbo].[Bookings];
DROP TABLE [dbo].[HoldSeats];
DROP TABLE [dbo].[SeatHolds];
DROP TABLE [dbo].[TripSeats];
DROP TABLE [dbo].[Trips];
DROP TABLE [dbo].[Vehicles];
DROP TABLE [dbo].[PhysicalSeats];
DROP TABLE [dbo].[Layouts];
DROP TABLE [dbo].[Routes];
DROP TABLE [dbo].[AspNetUserTokens];
DROP TABLE [dbo].[AspNetUserLogins];
DROP TABLE [dbo].[AspNetRoleClaims];
DROP TABLE [dbo].[AspNetUserClaims];
DROP TABLE [dbo].[AspNetUserRoles];
DROP TABLE [dbo].[AspNetRoles];
DROP TABLE [dbo].[AspNetUsers];
""");
 }
 protected override void BuildTargetModel(ModelBuilder modelBuilder)=>InitialModelV1.Build(modelBuilder);
}
