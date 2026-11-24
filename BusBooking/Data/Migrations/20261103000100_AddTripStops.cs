using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace BusBooking.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261103000100_AddTripStops")]
public sealed class AddTripStops : Migration {
    protected override void Up(MigrationBuilder m) {
        m.CreateTable(name:"TripStops",columns:t=>new {
            Id=t.Column<int>("int",nullable:false).Annotation("SqlServer:Identity","1, 1"),
            TripId=t.Column<int>("int",nullable:false),StopOrder=t.Column<int>("int",nullable:false),
            Name=t.Column<string>("nvarchar(160)",maxLength:160,nullable:false),
            Address=t.Column<string>("nvarchar(500)",maxLength:500,nullable:true),
            OffsetMinutes=t.Column<int>("int",nullable:false),CanPickup=t.Column<bool>("bit",nullable:false),
            CanDropoff=t.Column<bool>("bit",nullable:false),IsActive=t.Column<bool>("bit",nullable:false),IsIllustrative=t.Column<bool>("bit",nullable:false)
        },constraints:t=> {
            t.PrimaryKey("PK_TripStops",x=>x.Id);
            t.CheckConstraint("CK_TripStop_Offset","[OffsetMinutes] >= 0");
            t.ForeignKey("FK_TripStops_Trips_TripId",x=>x.TripId,"Trips","Id",onDelete:ReferentialAction.Restrict);
        });
        m.CreateIndex("IX_TripStops_TripId_StopOrder","TripStops",new[]{"TripId","StopOrder"},unique:true);
        foreach(var table in new[]{"SeatHolds","Bookings"})foreach(var col in new[]{"PickupStopId","DropoffStopId"}) {
            m.AddColumn<int>(col,table,"int",nullable:true);
            m.CreateIndex($"IX_{table}_{col}",table,col);
            m.AddForeignKey($"FK_{table}_TripStops_{col}",table,col,"TripStops",principalColumn:"Id",onDelete:ReferentialAction.Restrict);
        }
        foreach(var col in new[]{"PickupName","DropoffName"})m.AddColumn<string>(col,"Bookings","nvarchar(160)",maxLength:160,nullable:true);
        foreach(var col in new[]{"PickupAddress","DropoffAddress"})m.AddColumn<string>(col,"Bookings","nvarchar(500)",maxLength:500,nullable:true);
        foreach(var col in new[]{"PickupAtUtc","DropoffAtUtc"})m.AddColumn<DateTime>(col,"Bookings","datetime2",nullable:true);
        m.AddColumn<bool>("StopsIllustrative","Bookings","bit",nullable:true);
        // Only INSERT stop metadata. Never update a trip, seat, hold, booking, payment or identity row.
        m.Sql("""
            INSERT INTO [TripStops] ([TripId],[StopOrder],[Name],[OffsetMinutes],[CanPickup],[CanDropoff],[IsActive],[IsIllustrative])
            SELECT t.[Id],0,N'Điểm đầu · '+r.[Origin],0,1,0,1,1 FROM [Trips] t INNER JOIN [Routes] r ON r.[Id]=t.[BusRouteId];
            INSERT INTO [TripStops] ([TripId],[StopOrder],[Name],[OffsetMinutes],[CanPickup],[CanDropoff],[IsActive],[IsIllustrative])
            SELECT t.[Id],1000,N'Điểm cuối · '+r.[Destination],CAST(DATEDIFF_BIG(second,t.[DepartureUtc],t.[ArrivalUtc])/60 AS int),0,1,1,1
            FROM [Trips] t INNER JOIN [Routes] r ON r.[Id]=t.[BusRouteId];
            """);
    }
    protected override void Down(MigrationBuilder m) {
        foreach(var table in new[]{"SeatHolds","Bookings"})foreach(var col in new[]{"PickupStopId","DropoffStopId"}) {
            m.DropForeignKey($"FK_{table}_TripStops_{col}",table);m.DropIndex($"IX_{table}_{col}",table);m.DropColumn(col,table);
        }
        foreach(var col in new[]{"PickupName","DropoffName","PickupAddress","DropoffAddress","PickupAtUtc","DropoffAtUtc","StopsIllustrative"})m.DropColumn(col,"Bookings");
        m.DropTable("TripStops");
    }
    protected override void BuildTargetModel(ModelBuilder b) {
        InitialModelV1.Build(b);ContactModelV1.Build(b);StopModelV1.Build(b);
    }
}
