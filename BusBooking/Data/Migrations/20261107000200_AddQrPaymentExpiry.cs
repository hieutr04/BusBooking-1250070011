using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BusBooking.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261107000200_AddQrPaymentExpiry")]
public sealed class AddQrPaymentExpiry : Migration {
    private const string OldConstraint = "[Total]>=0 AND [Status] IN ('Active','Cancelled','Exchanged')";
    private const string NewConstraint = "[Total]>=0 AND [Status] IN ('Active','Cancelled','Exchanged','Expired')";

    protected override void Up(MigrationBuilder m) {
        m.AddColumn<DateTime>("PaymentExpiresUtc", "Bookings", "datetime2", nullable: true);
        m.CreateIndex("IX_Bookings_Status_PaymentExpiresUtc", "Bookings", new[]{"Status","PaymentExpiresUtc"},
            filter: "[PaymentExpiresUtc] IS NOT NULL");
        m.DropCheckConstraint("CK_Booking_Total", "Bookings");
        m.AddCheckConstraint("CK_Booking_Total", "Bookings", NewConstraint);
    }

    protected override void Down(MigrationBuilder m) {
        m.DropCheckConstraint("CK_Booking_Total", "Bookings");
        m.Sql("UPDATE dbo.Bookings SET Status=N'Cancelled' WHERE Status=N'Expired';");
        m.AddCheckConstraint("CK_Booking_Total", "Bookings", OldConstraint);
        m.DropIndex("IX_Bookings_Status_PaymentExpiresUtc", "Bookings");
        m.DropColumn("PaymentExpiresUtc", "Bookings");
    }

    protected override void BuildTargetModel(ModelBuilder model) {
        InitialModelV1.Build(model);
        ContactModelV1.Build(model);
        StopModelV1.Build(model);
        model.Entity("BusBooking.Models.Ticket").Property<DateTime?>("CheckedInUtc").HasColumnType("datetime2");
        AddTripCoDriver.Apply(model);
        AddDriverProfileAndVehicleType.Apply(model);
        AddSimulatedPayment.Apply(model);
        AddUserContactProfile.Apply(model);
        Apply(model);
    }

    internal static void Apply(ModelBuilder model) {
        model.Entity("BusBooking.Models.Booking", booking => {
            booking.Property<DateTime?>("PaymentExpiresUtc").HasColumnType("datetime2");
            booking.HasIndex("Status", "PaymentExpiresUtc").HasFilter("[PaymentExpiresUtc] IS NOT NULL");
            booking.ToTable("Bookings", table => table.HasCheckConstraint("CK_Booking_Total", NewConstraint));
        });
    }
}
