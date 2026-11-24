using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace BusBooking.Data.Migrations;
[DbContext(typeof(AppDbContext))]
public class AppDbContextModelSnapshot:ModelSnapshot {
    protected override void BuildModel(ModelBuilder modelBuilder) {
        InitialModelV1.Build(modelBuilder);
        ContactModelV1.Build(modelBuilder);
        StopModelV1.Build(modelBuilder);
        modelBuilder.Entity("BusBooking.Models.Ticket").Property<DateTime?>("CheckedInUtc").HasColumnType("datetime2");
        AddTripCoDriver.Apply(modelBuilder);
        AddDriverProfileAndVehicleType.Apply(modelBuilder);
        AddSimulatedPayment.Apply(modelBuilder);
        AddUserContactProfile.Apply(modelBuilder);
        AddQrPaymentExpiry.Apply(modelBuilder);
    }
}
