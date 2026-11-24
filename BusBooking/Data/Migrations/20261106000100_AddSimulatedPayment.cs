using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BusBooking.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261106000100_AddSimulatedPayment")]
public sealed class AddSimulatedPayment : Migration {
    private const string Constraint="([SimulatedPaidUtc] IS NULL AND [SimulatedPaidAmount] IS NULL) OR ([SimulatedPaidUtc] IS NOT NULL AND [SimulatedPaidAmount] IS NOT NULL AND [SimulatedPaidAmount]>0 AND [SimulatedPaidAmount]<=[Total])";

    protected override void Up(MigrationBuilder m) {
        m.AddColumn<DateTime>("SimulatedPaidUtc","Bookings","datetime2",nullable:true);
        m.AddColumn<decimal>("SimulatedPaidAmount","Bookings","decimal(18,2)",precision:18,scale:2,nullable:true);
        m.AddCheckConstraint("CK_Booking_SimulatedPayment","Bookings",Constraint);
    }

    protected override void Down(MigrationBuilder m) {
        m.DropCheckConstraint("CK_Booking_SimulatedPayment","Bookings");
        m.DropColumn("SimulatedPaidAmount","Bookings");
        m.DropColumn("SimulatedPaidUtc","Bookings");
    }

    protected override void BuildTargetModel(ModelBuilder model) {
        InitialModelV1.Build(model);
        ContactModelV1.Build(model);
        StopModelV1.Build(model);
        model.Entity("BusBooking.Models.Ticket").Property<DateTime?>("CheckedInUtc").HasColumnType("datetime2");
        AddTripCoDriver.Apply(model);
        AddDriverProfileAndVehicleType.Apply(model);
        Apply(model);
    }

    internal static void Apply(ModelBuilder model) {
        model.Entity("BusBooking.Models.Booking", booking=>{
            booking.Property<DateTime?>("SimulatedPaidUtc").HasColumnType("datetime2");
            booking.Property<decimal?>("SimulatedPaidAmount").HasPrecision(18,2).HasColumnType("decimal(18,2)");
            booking.ToTable("Bookings",table=>table.HasCheckConstraint("CK_Booking_SimulatedPayment",Constraint));
        });
    }
}
