using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BusBooking.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261104000100_AddTicketCheckIn")]
public sealed class AddTicketCheckIn : Migration {
    protected override void Up(MigrationBuilder migrationBuilder)=>
        migrationBuilder.AddColumn<DateTime>("CheckedInUtc","Tickets","datetime2",nullable:true);

    protected override void Down(MigrationBuilder migrationBuilder)=>
        migrationBuilder.DropColumn("CheckedInUtc","Tickets");

    protected override void BuildTargetModel(ModelBuilder modelBuilder) {
        InitialModelV1.Build(modelBuilder);
        ContactModelV1.Build(modelBuilder);
        StopModelV1.Build(modelBuilder);
        modelBuilder.Entity("BusBooking.Models.Ticket").Property<DateTime?>("CheckedInUtc").HasColumnType("datetime2");
    }
}
