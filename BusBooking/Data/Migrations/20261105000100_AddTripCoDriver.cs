using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BusBooking.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261105000100_AddTripCoDriver")]
public sealed class AddTripCoDriver : Migration {
    protected override void Up(MigrationBuilder m) {
        m.AddColumn<string>("CoDriverId","Trips","nvarchar(450)",maxLength:450,nullable:true);
        m.CreateIndex("IX_Trips_CoDriverId_DepartureUtc_ArrivalUtc","Trips",new[]{"CoDriverId","DepartureUtc","ArrivalUtc"});
        m.AddForeignKey("FK_Trips_AspNetUsers_CoDriverId","Trips","CoDriverId","AspNetUsers",principalColumn:"Id",onDelete:ReferentialAction.Restrict);
    }
    protected override void Down(MigrationBuilder m) {
        m.DropForeignKey("FK_Trips_AspNetUsers_CoDriverId","Trips");
        m.DropIndex("IX_Trips_CoDriverId_DepartureUtc_ArrivalUtc","Trips");
        m.DropColumn("CoDriverId","Trips");
    }
    protected override void BuildTargetModel(ModelBuilder model) {
        InitialModelV1.Build(model);
        ContactModelV1.Build(model);
        StopModelV1.Build(model);
        model.Entity("BusBooking.Models.Ticket").Property<DateTime?>("CheckedInUtc").HasColumnType("datetime2");
        Apply(model);
    }
    internal static void Apply(ModelBuilder model) {
        model.Entity("BusBooking.Models.Trip",trip=>{
            trip.Property<string>("CoDriverId").HasColumnType("nvarchar(450)");
            trip.HasIndex("CoDriverId","DepartureUtc","ArrivalUtc");
            trip.HasOne("BusBooking.Models.AppUser","CoDriver").WithMany().HasForeignKey("CoDriverId").OnDelete(DeleteBehavior.Restrict);
        });
    }
}
