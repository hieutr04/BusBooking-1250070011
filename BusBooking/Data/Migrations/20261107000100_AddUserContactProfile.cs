using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BusBooking.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261107000100_AddUserContactProfile")]
public sealed class AddUserContactProfile : Migration {
    protected override void Up(MigrationBuilder m) {
        m.AddColumn<string>("Address","AspNetUsers","nvarchar(300)",maxLength:300,nullable:true);
        m.AddColumn<string>("ProvinceCode","AspNetUsers","nvarchar(2)",maxLength:2,nullable:true);
        // Earlier versions did not store this date. Existing profiles receive the migration timestamp.
        m.AddColumn<DateTime>("CreatedUtc","AspNetUsers","datetime2",nullable:false,defaultValueSql:"SYSUTCDATETIME()");
    }

    protected override void Down(MigrationBuilder m) {
        m.DropColumn("CreatedUtc","AspNetUsers");
        m.DropColumn("ProvinceCode","AspNetUsers");
        m.DropColumn("Address","AspNetUsers");
    }

    protected override void BuildTargetModel(ModelBuilder model) {
        InitialModelV1.Build(model);
        ContactModelV1.Build(model);
        StopModelV1.Build(model);
        model.Entity("BusBooking.Models.Ticket").Property<DateTime?>("CheckedInUtc").HasColumnType("datetime2");
        AddTripCoDriver.Apply(model);
        AddDriverProfileAndVehicleType.Apply(model);
        AddSimulatedPayment.Apply(model);
        Apply(model);
    }

    internal static void Apply(ModelBuilder model) {
        model.Entity("BusBooking.Models.AppUser",user=>{
            user.Property<string>("Address").HasMaxLength(300).HasColumnType("nvarchar(300)");
            user.Property<string>("ProvinceCode").HasMaxLength(2).HasColumnType("nvarchar(2)");
            user.Property<DateTime>("CreatedUtc").ValueGeneratedOnAdd().HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
        });
    }
}
