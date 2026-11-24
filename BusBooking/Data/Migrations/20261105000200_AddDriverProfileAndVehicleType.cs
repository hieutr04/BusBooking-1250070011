using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BusBooking.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261105000200_AddDriverProfileAndVehicleType")]
public sealed class AddDriverProfileAndVehicleType : Migration {
    protected override void Up(MigrationBuilder m) {
        m.AddColumn<DateOnly>("DateOfBirth","AspNetUsers","date",nullable:true);
        m.AddColumn<string>("Kind","Vehicles","nvarchar(12)",maxLength:12,nullable:true);
        m.Sql("""
            EXEC(N'UPDATE v SET Kind=CASE WHEN l.Floors>1 OR l.Name LIKE N''%giường nằm%''
                                         THEN N''Sleeper'' ELSE N''Seated'' END
                   FROM dbo.Vehicles v INNER JOIN dbo.Layouts l ON l.Id=v.SeatLayoutId');
            """);
        m.AlterColumn<string>("Kind","Vehicles","nvarchar(12)",maxLength:12,nullable:false,
            oldClrType:typeof(string),oldType:"nvarchar(12)",oldMaxLength:12,oldNullable:true);
        m.AddCheckConstraint("CK_Vehicle_Kind","Vehicles","[Kind] IN ('Seated','Sleeper')");
        m.Sql("""
            UPDATE u SET FullName=CASE u.NormalizedEmail
                WHEN N'FLEET-DRIVER-01@BUSBOOKING.TEST' THEN N'Nguyễn Văn A'
                WHEN N'FLEET-DRIVER-02@BUSBOOKING.TEST' THEN N'Trần Bảo A' END
            FROM dbo.AspNetUsers u
            INNER JOIN dbo.AspNetUserRoles ur ON ur.UserId=u.Id
            INNER JOIN dbo.AspNetRoles r ON r.Id=ur.RoleId AND r.NormalizedName=N'DRIVER'
            WHERE (u.NormalizedEmail=N'FLEET-DRIVER-01@BUSBOOKING.TEST' AND u.FullName=N'Tài xế đội xe 01')
               OR (u.NormalizedEmail=N'FLEET-DRIVER-02@BUSBOOKING.TEST' AND u.FullName=N'Tài xế đội xe 02');
            """);
    }

    protected override void Down(MigrationBuilder m) {
        m.DropCheckConstraint("CK_Vehicle_Kind","Vehicles");
        m.DropColumn("Kind","Vehicles");
        m.DropColumn("DateOfBirth","AspNetUsers");
    }

    protected override void BuildTargetModel(ModelBuilder model) {
        InitialModelV1.Build(model);
        ContactModelV1.Build(model);
        StopModelV1.Build(model);
        model.Entity("BusBooking.Models.Ticket").Property<DateTime?>("CheckedInUtc").HasColumnType("datetime2");
        AddTripCoDriver.Apply(model);
        Apply(model);
    }

    internal static void Apply(ModelBuilder model) {
        model.Entity("BusBooking.Models.AppUser").Property<DateOnly?>("DateOfBirth").HasColumnType("date");
        model.Entity("BusBooking.Models.Vehicle", vehicle => {
            vehicle.Property<string>("Kind").IsRequired().HasMaxLength(12).HasColumnType("nvarchar(12)");
            vehicle.ToTable("Vehicles",table=>table.HasCheckConstraint("CK_Vehicle_Kind","[Kind] IN ('Seated','Sleeper')"));
        });
    }
}
