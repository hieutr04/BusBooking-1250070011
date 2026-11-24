using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
namespace BusBooking.Data.Migrations;
// Frozen initial model. Keep this version immutable when adding future migrations.
internal static class InitialModelV1 {
 public static void Build(ModelBuilder modelBuilder) {
  modelBuilder.HasAnnotation("ProductVersion", "9.0.9").HasAnnotation("Relational:MaxIdentifierLength", 128);
  SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);
  modelBuilder.Entity("BusBooking.Models.AppUser", b => {
   b.Property<string>("Id").IsRequired().HasColumnType("nvarchar(450)");
   b.Property<string>("FullName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
   b.Property<bool>("IsActive").HasColumnType("bit");
   b.Property<string>("UserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
   b.Property<string>("NormalizedUserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
   b.Property<string>("Email").HasMaxLength(256).HasColumnType("nvarchar(256)");
   b.Property<string>("NormalizedEmail").HasMaxLength(256).HasColumnType("nvarchar(256)");
   b.Property<bool>("EmailConfirmed").HasColumnType("bit");
   b.Property<string>("PasswordHash").HasColumnType("nvarchar(max)");
   b.Property<string>("SecurityStamp").HasColumnType("nvarchar(max)");
   b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("nvarchar(max)");
   b.Property<string>("PhoneNumber").HasColumnType("nvarchar(max)");
   b.Property<bool>("PhoneNumberConfirmed").HasColumnType("bit");
   b.Property<bool>("TwoFactorEnabled").HasColumnType("bit");
   b.Property<DateTimeOffset?>("LockoutEnd").HasColumnType("datetimeoffset");
   b.Property<bool>("LockoutEnabled").HasColumnType("bit");
   b.Property<int>("AccessFailedCount").HasColumnType("int");
   b.HasKey("Id");
   b.HasIndex(new[] { "NormalizedEmail" }).HasDatabaseName("EmailIndex");
   b.HasIndex(new[] { "NormalizedUserName" }).IsUnique().HasDatabaseName("UserNameIndex").HasFilter("[NormalizedUserName] IS NOT NULL");
   b.ToTable("AspNetUsers", (string?)null);
  });
  modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b => {
   b.Property<string>("Id").IsRequired().HasColumnType("nvarchar(450)");
   b.Property<string>("Name").HasMaxLength(256).HasColumnType("nvarchar(256)");
   b.Property<string>("NormalizedName").HasMaxLength(256).HasColumnType("nvarchar(256)");
   b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("nvarchar(max)");
   b.HasKey("Id");
   b.HasIndex(new[] { "NormalizedName" }).IsUnique().HasDatabaseName("RoleNameIndex").HasFilter("[NormalizedName] IS NOT NULL");
   b.ToTable("AspNetRoles", (string?)null);
  });
  modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b => {
   b.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
   b.Property<string>("RoleId").IsRequired().HasColumnType("nvarchar(450)");
   b.HasKey("UserId", "RoleId");
   b.HasIndex(new[] { "RoleId" });
   b.ToTable("AspNetUserRoles", (string?)null);
  });
  modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b => {
   b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
   b.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
   b.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
   b.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
   b.HasKey("Id");
   b.HasIndex(new[] { "UserId" });
   b.ToTable("AspNetUserClaims", (string?)null);
  });
  modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b => {
   b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
   b.Property<string>("RoleId").IsRequired().HasColumnType("nvarchar(450)");
   b.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
   b.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
   b.HasKey("Id");
   b.HasIndex(new[] { "RoleId" });
   b.ToTable("AspNetRoleClaims", (string?)null);
  });
  modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b => {
   b.Property<string>("LoginProvider").IsRequired().HasMaxLength(128).HasColumnType("nvarchar(128)");
   b.Property<string>("ProviderKey").IsRequired().HasMaxLength(128).HasColumnType("nvarchar(128)");
   b.Property<string>("ProviderDisplayName").HasColumnType("nvarchar(max)");
   b.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
   b.HasKey("LoginProvider", "ProviderKey");
   b.HasIndex(new[] { "UserId" });
   b.ToTable("AspNetUserLogins", (string?)null);
  });
  modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b => {
   b.Property<string>("UserId").IsRequired().HasColumnType("nvarchar(450)");
   b.Property<string>("LoginProvider").IsRequired().HasMaxLength(128).HasColumnType("nvarchar(128)");
   b.Property<string>("Name").IsRequired().HasMaxLength(128).HasColumnType("nvarchar(128)");
   b.Property<string>("Value").HasColumnType("nvarchar(max)");
   b.HasKey("UserId", "LoginProvider", "Name");
   b.ToTable("AspNetUserTokens", (string?)null);
  });
  modelBuilder.Entity("BusBooking.Models.BusRoute", b => {
   b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
   b.Property<string>("Origin").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
   b.Property<string>("Destination").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
   b.Property<string>("Description").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
   b.Property<int>("DurationMinutes").HasColumnType("int");
   b.Property<bool>("IsActive").HasColumnType("bit");
   b.HasKey("Id");
   b.HasIndex(new[] { "Origin", "Destination" }).IsUnique();
   b.ToTable("Routes", (string?)null, t => {
    t.HasCheckConstraint("CK_Route_Duration", "[DurationMinutes] > 0 AND [Origin] <> [Destination]");
   });
  });
  modelBuilder.Entity("BusBooking.Models.SeatLayout", b => {
   b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
   b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
   b.Property<int>("Rows").HasColumnType("int");
   b.Property<int>("Columns").HasColumnType("int");
   b.Property<int>("Floors").HasColumnType("int");
   b.Property<bool>("IsActive").HasColumnType("bit");
   b.HasKey("Id");
   b.ToTable("Layouts", (string?)null, t => {
    t.HasCheckConstraint("CK_Layout_Shape", "[Rows] BETWEEN 1 AND 20 AND [Columns] BETWEEN 1 AND 4 AND [Floors] BETWEEN 1 AND 2");
   });
  });
  modelBuilder.Entity("BusBooking.Models.PhysicalSeat", b => {
   b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
   b.Property<int>("SeatLayoutId").HasColumnType("int");
   b.Property<string>("Code").IsRequired().HasMaxLength(10).HasColumnType("nvarchar(10)");
   b.Property<int>("Row").HasColumnType("int");
   b.Property<int>("Column").HasColumnType("int");
   b.Property<int>("Floor").HasColumnType("int");
   b.HasKey("Id");
   b.HasIndex(new[] { "SeatLayoutId", "Code" }).IsUnique();
   b.HasIndex(new[] { "SeatLayoutId", "Floor", "Row", "Column" }).IsUnique();
   b.ToTable("PhysicalSeats", (string?)null, t => {
    t.HasCheckConstraint("CK_PhysicalSeat_Position", "[Row]>0 AND [Column]>0 AND [Floor]>0");
   });
  });
  modelBuilder.Entity("BusBooking.Models.Vehicle", b => {
   b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
   b.Property<string>("Plate").IsRequired().HasMaxLength(30).HasColumnType("nvarchar(30)");
   b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
   b.Property<int>("SeatLayoutId").HasColumnType("int");
   b.Property<bool>("IsActive").HasColumnType("bit");
   b.HasKey("Id");
   b.HasIndex(new[] { "Plate" }).IsUnique();
   b.HasIndex(new[] { "SeatLayoutId" });
   b.ToTable("Vehicles", (string?)null);
  });
  modelBuilder.Entity("BusBooking.Models.Trip", b => {
   b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
   b.Property<int>("BusRouteId").HasColumnType("int");
   b.Property<int>("VehicleId").HasColumnType("int");
   b.Property<DateTime>("DepartureUtc").HasColumnType("datetime2");
   b.Property<DateTime>("ArrivalUtc").HasColumnType("datetime2");
   b.Property<decimal>("Fare").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
   b.Property<string>("Status").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
   b.Property<bool>("SalesOpen").HasColumnType("bit");
   b.Property<long>("Revision").HasColumnType("bigint");
   b.Property<string>("DriverId").HasMaxLength(450).HasColumnType("nvarchar(450)");
   b.Property<string>("StaffId").HasMaxLength(450).HasColumnType("nvarchar(450)");
   b.HasKey("Id");
   b.HasIndex(new[] { "BusRouteId", "DepartureUtc", "Status", "SalesOpen" });
   b.HasIndex(new[] { "VehicleId", "DepartureUtc", "ArrivalUtc" });
   b.HasIndex(new[] { "DriverId", "DepartureUtc", "ArrivalUtc" });
   b.HasIndex(new[] { "StaffId" });
   b.ToTable("Trips", (string?)null, t => {
    t.HasCheckConstraint("CK_Trip_Values", "[ArrivalUtc]>[DepartureUtc] AND [Fare]>=0 AND [Revision]>=0 AND [Status] IN ('Scheduled','Boarding','InProgress','Completed','Cancelled')");
   });
  });
  modelBuilder.Entity("BusBooking.Models.TripSeat", b => {
   b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
   b.Property<int>("TripId").HasColumnType("int");
   b.Property<int>("PhysicalSeatId").HasColumnType("int");
   b.Property<string>("Code").IsRequired().HasMaxLength(10).HasColumnType("nvarchar(10)");
   b.Property<int>("Row").HasColumnType("int");
   b.Property<int>("Column").HasColumnType("int");
   b.Property<int>("Floor").HasColumnType("int");
   b.Property<string>("Status").IsRequired().HasMaxLength(12).HasColumnType("nvarchar(12)");
   b.Property<Guid?>("CurrentHoldId").HasColumnType("uniqueidentifier");
   b.HasKey("Id");
   b.HasAlternateKey("Id", "TripId");
   b.HasIndex(new[] { "TripId", "PhysicalSeatId" }).IsUnique();
   b.HasIndex(new[] { "TripId", "Code" }).IsUnique();
   b.HasIndex(new[] { "TripId", "Status" });
   b.HasIndex(new[] { "PhysicalSeatId" });
   b.HasIndex(new[] { "CurrentHoldId", "TripId" });
   b.ToTable("TripSeats", (string?)null, t => {
    t.HasCheckConstraint("CK_TripSeat_State", "([Status]='Held' AND [CurrentHoldId] IS NOT NULL) OR ([Status] IN ('Available','Booked') AND [CurrentHoldId] IS NULL)");
   });
  });
  modelBuilder.Entity("BusBooking.Models.SeatHold", b => {
   b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
   b.Property<int>("TripId").HasColumnType("int");
   b.Property<string>("OwnerId").IsRequired().HasMaxLength(450).HasColumnType("nvarchar(450)");
   b.Property<Guid>("RequestKey").HasColumnType("uniqueidentifier");
   b.Property<DateTime>("CreatedUtc").HasColumnType("datetime2");
   b.Property<DateTime>("ExpiresAtUtc").HasColumnType("datetime2");
   b.Property<string>("Status").IsRequired().HasMaxLength(12).HasColumnType("nvarchar(12)");
   b.HasKey("Id");
   b.HasAlternateKey("Id", "TripId");
   b.HasIndex(new[] { "TripId" });
   b.HasIndex(new[] { "OwnerId", "RequestKey" }).IsUnique();
   b.HasIndex(new[] { "Status", "ExpiresAtUtc", "TripId" });
   b.ToTable("SeatHolds", (string?)null, t => {
    t.HasCheckConstraint("CK_Hold_Expiry", "[ExpiresAtUtc]>[CreatedUtc] AND [Status] IN ('Active','Released','Expired','Confirmed')");
   });
  });
  modelBuilder.Entity("BusBooking.Models.HoldSeat", b => {
   b.Property<Guid>("SeatHoldId").HasColumnType("uniqueidentifier");
   b.Property<int>("TripSeatId").HasColumnType("int");
   b.Property<int>("TripId").HasColumnType("int");
   b.HasKey("SeatHoldId", "TripSeatId");
   b.HasIndex(new[] { "SeatHoldId", "TripId" });
   b.HasIndex(new[] { "TripSeatId", "TripId" });
   b.ToTable("HoldSeats", (string?)null);
  });
  modelBuilder.Entity("BusBooking.Models.Booking", b => {
   b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
   b.Property<Guid>("SeatHoldId").HasColumnType("uniqueidentifier");
   b.Property<string>("Code").IsRequired().HasMaxLength(30).HasColumnType("nvarchar(30)");
   b.Property<string>("OwnerId").IsRequired().HasMaxLength(450).HasColumnType("nvarchar(450)");
   b.Property<string>("ContactName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
   b.Property<string>("ContactPhone").IsRequired().HasMaxLength(25).HasColumnType("nvarchar(25)");
   b.Property<DateTime>("CreatedUtc").HasColumnType("datetime2");
   b.Property<string>("Status").IsRequired().HasMaxLength(12).HasColumnType("nvarchar(12)");
   b.Property<decimal>("Total").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
   b.Property<Guid?>("ReplacesBookingId").HasColumnType("uniqueidentifier");
   b.HasKey("Id");
   b.HasIndex(new[] { "SeatHoldId" }).IsUnique();
   b.HasIndex(new[] { "Code" }).IsUnique();
   b.HasIndex(new[] { "OwnerId", "CreatedUtc" });
   b.HasIndex(new[] { "Status", "CreatedUtc" });
   b.HasIndex(new[] { "ReplacesBookingId" });
   b.ToTable("Bookings", (string?)null, t => {
    t.HasCheckConstraint("CK_Booking_Total", "[Total]>=0 AND [Status] IN ('Active','Cancelled','Exchanged')");
   });
  });
  modelBuilder.Entity("BusBooking.Models.Ticket", b => {
   b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
   b.Property<Guid>("BookingId").HasColumnType("uniqueidentifier");
   b.Property<int>("TripSeatId").HasColumnType("int");
   b.Property<string>("PassengerName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
   b.Property<decimal>("Fare").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
   b.Property<bool>("IsActive").HasColumnType("bit");
   b.HasKey("Id");
   b.HasIndex(new[] { "BookingId" });
   b.HasIndex(new[] { "TripSeatId" }).IsUnique().HasFilter("[IsActive] = 1");
   b.ToTable("Tickets", (string?)null, t => {
    t.HasCheckConstraint("CK_Ticket_Fare", "[Fare]>=0");
   });
  });
  modelBuilder.Entity("BusBooking.Models.PaymentEntry", b => {
   b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
   b.Property<Guid>("BookingId").HasColumnType("uniqueidentifier");
   b.Property<string>("Kind").IsRequired().HasMaxLength(12).HasColumnType("nvarchar(12)");
   b.Property<decimal>("Amount").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
   b.Property<string>("ActorId").IsRequired().HasMaxLength(450).HasColumnType("nvarchar(450)");
   b.Property<Guid>("RequestKey").HasColumnType("uniqueidentifier");
   b.Property<DateTime>("CreatedUtc").HasColumnType("datetime2");
   b.Property<string>("Note").IsRequired().HasMaxLength(300).HasColumnType("nvarchar(300)");
   b.HasKey("Id");
   b.HasIndex(new[] { "BookingId" });
   b.HasIndex(new[] { "ActorId", "RequestKey" }).IsUnique();
   b.HasIndex(new[] { "CreatedUtc" });
   b.ToTable("Payments", (string?)null, t => {
    t.HasCheckConstraint("CK_Payment_Sign", "([Kind]='Receipt' AND [Amount]>0) OR ([Kind]='Refund' AND [Amount]<0)");
   });
  });
  modelBuilder.Entity("BusBooking.Models.ChangeRequest", b => {
   b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
   b.Property<Guid>("BookingId").HasColumnType("uniqueidentifier");
   b.Property<string>("Kind").IsRequired().HasMaxLength(12).HasColumnType("nvarchar(12)");
   b.Property<string>("Status").IsRequired().HasMaxLength(12).HasColumnType("nvarchar(12)");
   b.Property<string>("Reason").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
   b.Property<string>("Resolution").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
   b.Property<DateTime>("CreatedUtc").HasColumnType("datetime2");
   b.Property<DateTime?>("ResolvedUtc").HasColumnType("datetime2");
   b.Property<string>("ResolvedBy").HasMaxLength(450).HasColumnType("nvarchar(450)");
   b.HasKey("Id");
   b.HasIndex(new[] { "BookingId" }).IsUnique().HasFilter("[Status] = 'Pending'");
   b.ToTable("ChangeRequests", (string?)null);
  });
  modelBuilder.Entity("BusBooking.Models.AuditEntry", b => {
   b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("bigint").UseIdentityColumn();
   b.Property<DateTime>("CreatedUtc").HasColumnType("datetime2");
   b.Property<string>("ActorId").IsRequired().HasMaxLength(450).HasColumnType("nvarchar(450)");
   b.Property<string>("Action").IsRequired().HasMaxLength(60).HasColumnType("nvarchar(60)");
   b.Property<string>("EntityId").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
   b.Property<string>("Detail").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
   b.HasKey("Id");
   b.HasIndex(new[] { "Action", "CreatedUtc" });
   b.ToTable("AuditEntries", (string?)null);
  });
  modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b => {
   b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null).WithMany(null).HasForeignKey("RoleId").OnDelete(DeleteBehavior.Restrict).IsRequired();
   b.HasOne("BusBooking.Models.AppUser", null).WithMany(null).HasForeignKey("UserId").OnDelete(DeleteBehavior.Restrict).IsRequired();
  });
  modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b => {
   b.HasOne("BusBooking.Models.AppUser", null).WithMany(null).HasForeignKey("UserId").OnDelete(DeleteBehavior.Restrict).IsRequired();
  });
  modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b => {
   b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null).WithMany(null).HasForeignKey("RoleId").OnDelete(DeleteBehavior.Restrict).IsRequired();
  });
  modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b => {
   b.HasOne("BusBooking.Models.AppUser", null).WithMany(null).HasForeignKey("UserId").OnDelete(DeleteBehavior.Restrict).IsRequired();
  });
  modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b => {
   b.HasOne("BusBooking.Models.AppUser", null).WithMany(null).HasForeignKey("UserId").OnDelete(DeleteBehavior.Restrict).IsRequired();
  });
  modelBuilder.Entity("BusBooking.Models.PhysicalSeat", b => {
   b.HasOne("BusBooking.Models.SeatLayout", "Layout").WithMany("Seats").HasForeignKey("SeatLayoutId").OnDelete(DeleteBehavior.Restrict).IsRequired();
  });
  modelBuilder.Entity("BusBooking.Models.Vehicle", b => {
   b.HasOne("BusBooking.Models.SeatLayout", "Layout").WithMany(null).HasForeignKey("SeatLayoutId").OnDelete(DeleteBehavior.Restrict).IsRequired();
  });
  modelBuilder.Entity("BusBooking.Models.Trip", b => {
   b.HasOne("BusBooking.Models.BusRoute", "Route").WithMany(null).HasForeignKey("BusRouteId").OnDelete(DeleteBehavior.Restrict).IsRequired();
   b.HasOne("BusBooking.Models.Vehicle", "Vehicle").WithMany(null).HasForeignKey("VehicleId").OnDelete(DeleteBehavior.Restrict).IsRequired();
   b.HasOne("BusBooking.Models.AppUser", "Driver").WithMany(null).HasForeignKey("DriverId").OnDelete(DeleteBehavior.Restrict);
   b.HasOne("BusBooking.Models.AppUser", "Staff").WithMany(null).HasForeignKey("StaffId").OnDelete(DeleteBehavior.Restrict);
  });
  modelBuilder.Entity("BusBooking.Models.TripSeat", b => {
   b.HasOne("BusBooking.Models.Trip", "Trip").WithMany("Seats").HasForeignKey("TripId").OnDelete(DeleteBehavior.Restrict).IsRequired();
   b.HasOne("BusBooking.Models.PhysicalSeat", "PhysicalSeat").WithMany(null).HasForeignKey("PhysicalSeatId").OnDelete(DeleteBehavior.Restrict).IsRequired();
   b.HasOne("BusBooking.Models.SeatHold", "CurrentHold").WithMany(null).HasForeignKey("CurrentHoldId", "TripId").HasPrincipalKey("Id", "TripId").OnDelete(DeleteBehavior.Restrict);
  });
  modelBuilder.Entity("BusBooking.Models.SeatHold", b => {
   b.HasOne("BusBooking.Models.Trip", "Trip").WithMany(null).HasForeignKey("TripId").OnDelete(DeleteBehavior.Restrict).IsRequired();
   b.HasOne("BusBooking.Models.AppUser", "Owner").WithMany(null).HasForeignKey("OwnerId").OnDelete(DeleteBehavior.Restrict).IsRequired();
  });
  modelBuilder.Entity("BusBooking.Models.HoldSeat", b => {
   b.HasOne("BusBooking.Models.SeatHold", "Hold").WithMany("Items").HasForeignKey("SeatHoldId", "TripId").HasPrincipalKey("Id", "TripId").OnDelete(DeleteBehavior.Restrict).IsRequired();
   b.HasOne("BusBooking.Models.TripSeat", "Seat").WithMany(null).HasForeignKey("TripSeatId", "TripId").HasPrincipalKey("Id", "TripId").OnDelete(DeleteBehavior.Restrict).IsRequired();
  });
  modelBuilder.Entity("BusBooking.Models.Booking", b => {
   b.HasOne("BusBooking.Models.SeatHold", "Hold").WithMany(null).HasForeignKey("SeatHoldId").OnDelete(DeleteBehavior.Restrict).IsRequired();
   b.HasOne("BusBooking.Models.AppUser", "Owner").WithMany(null).HasForeignKey("OwnerId").OnDelete(DeleteBehavior.Restrict).IsRequired();
   b.HasOne("BusBooking.Models.Booking", null).WithMany(null).HasForeignKey("ReplacesBookingId").OnDelete(DeleteBehavior.Restrict);
  });
  modelBuilder.Entity("BusBooking.Models.Ticket", b => {
   b.HasOne("BusBooking.Models.Booking", "Booking").WithMany("Tickets").HasForeignKey("BookingId").OnDelete(DeleteBehavior.Restrict).IsRequired();
   b.HasOne("BusBooking.Models.TripSeat", "Seat").WithMany(null).HasForeignKey("TripSeatId").OnDelete(DeleteBehavior.Restrict).IsRequired();
  });
  modelBuilder.Entity("BusBooking.Models.PaymentEntry", b => {
   b.HasOne("BusBooking.Models.Booking", "Booking").WithMany("Payments").HasForeignKey("BookingId").OnDelete(DeleteBehavior.Restrict).IsRequired();
  });
  modelBuilder.Entity("BusBooking.Models.ChangeRequest", b => {
   b.HasOne("BusBooking.Models.Booking", "Booking").WithMany(null).HasForeignKey("BookingId").OnDelete(DeleteBehavior.Restrict).IsRequired();
  });
 }
}