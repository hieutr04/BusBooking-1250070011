using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace BusBooking.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261102000100_AddContactMessages")]
public sealed class AddContactMessages : Migration {
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.CreateTable(
            name: "ContactMessages",
            columns: table => new {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SubmissionKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                Phone = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                Subject = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                IsRead = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ContactMessages", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_ContactMessages_SubmissionKey", table: "ContactMessages", column: "SubmissionKey", unique: true);
        migrationBuilder.CreateIndex(name: "IX_ContactMessages_IsRead_CreatedUtc", table: "ContactMessages", columns: new[] { "IsRead", "CreatedUtc" });
    }
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "ContactMessages");
    protected override void BuildTargetModel(ModelBuilder modelBuilder) {
        InitialModelV1.Build(modelBuilder);
        ContactModelV1.Build(modelBuilder);
    }
}
