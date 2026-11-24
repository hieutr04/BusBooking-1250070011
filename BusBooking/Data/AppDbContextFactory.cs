using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace BusBooking.Data;
public class AppDbContextFactory:IDesignTimeDbContextFactory<AppDbContext> {
    public AppDbContext CreateDbContext(string[] args) {
        // Same model as runtime. Placeholder is only for generating migrations offline, never a fallback backend.
        var folder=File.Exists("appsettings.json")?Directory.GetCurrentDirectory():Path.Combine(Directory.GetCurrentDirectory(),"BusBooking");
        var cfg=new ConfigurationBuilder().SetBasePath(folder).AddJsonFile("appsettings.json",optional:true)
            .AddUserSecrets<AppDbContextFactory>(optional:true).AddEnvironmentVariables().Build();
        var connection=cfg.GetConnectionString("DefaultConnection")??"";
        if(string.IsNullOrWhiteSpace(connection))connection="Server=(localdb)\\MSSQLLocalDB;Database=BusBookingStudy;Trusted_Connection=True;TrustServerCertificate=True";
        return new(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connection).Options);
    }
}
