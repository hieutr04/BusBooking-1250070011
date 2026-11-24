using Microsoft.EntityFrameworkCore;
namespace BusBooking.Data;

// Frozen, additive contact model. Keep V1 immutable if contact evolves later.
internal static class ContactModelV1 {
    public static void Build(ModelBuilder modelBuilder) {
        modelBuilder.Entity("BusBooking.Models.ContactMessage", b => {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uniqueidentifier");
            b.Property<Guid>("SubmissionKey").HasColumnType("uniqueidentifier");
            b.Property<string>("FullName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("Email").IsRequired().HasMaxLength(254).HasColumnType("nvarchar(254)");
            b.Property<string>("Phone").HasMaxLength(25).HasColumnType("nvarchar(25)");
            b.Property<string>("Subject").IsRequired().HasMaxLength(160).HasColumnType("nvarchar(160)");
            b.Property<string>("Body").IsRequired().HasMaxLength(4000).HasColumnType("nvarchar(4000)");
            b.Property<DateTime>("CreatedUtc").HasColumnType("datetime2");
            b.Property<bool>("IsRead").HasColumnType("bit");
            b.HasKey("Id");
            b.HasIndex("SubmissionKey").IsUnique();
            b.HasIndex("IsRead", "CreatedUtc");
            b.ToTable("ContactMessages", (string?)null);
        });
    }
}
