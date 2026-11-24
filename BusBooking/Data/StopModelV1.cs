using Microsoft.EntityFrameworkCore;
namespace BusBooking.Data;

// Additive, frozen model fragment, shared by runtime and the new migration snapshot.
internal static class StopModelV1 {
    public static void Build(ModelBuilder b) {
        b.Entity("BusBooking.Models.TripStop",e=> {
            e.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
            e.Property<int>("TripId").HasColumnType("int");
            e.Property<int>("StopOrder").HasColumnType("int");
            e.Property<string>("Name").IsRequired().HasMaxLength(160).HasColumnType("nvarchar(160)");
            e.Property<string>("Address").HasMaxLength(500).HasColumnType("nvarchar(500)");
            e.Property<int>("OffsetMinutes").HasColumnType("int");
            e.Property<bool>("CanPickup").HasColumnType("bit");e.Property<bool>("CanDropoff").HasColumnType("bit");
            e.Property<bool>("IsActive").HasColumnType("bit");e.Property<bool>("IsIllustrative").HasColumnType("bit");
            e.HasKey("Id");e.HasIndex("TripId","StopOrder").IsUnique();
            e.ToTable("TripStops",t=>t.HasCheckConstraint("CK_TripStop_Offset","[OffsetMinutes] >= 0"));
            e.HasOne("BusBooking.Models.Trip",null).WithMany().HasForeignKey("TripId").OnDelete(DeleteBehavior.Restrict).IsRequired();
        });
        foreach(var entity in new[]{"BusBooking.Models.SeatHold","BusBooking.Models.Booking"}) {
            b.Entity(entity,e=> {
                foreach(var name in new[]{"PickupStopId","DropoffStopId"}) {
                    e.Property<int?>(name).HasColumnType("int");e.HasIndex(name);
                    e.HasOne("BusBooking.Models.TripStop",null).WithMany().HasForeignKey(name).OnDelete(DeleteBehavior.Restrict);
                }
            });
        }
        b.Entity("BusBooking.Models.Booking",e=> {
            foreach(var name in new[]{"PickupName","DropoffName"})e.Property<string>(name).HasMaxLength(160).HasColumnType("nvarchar(160)");
            foreach(var name in new[]{"PickupAddress","DropoffAddress"})e.Property<string>(name).HasMaxLength(500).HasColumnType("nvarchar(500)");
            e.Property<DateTime?>("PickupAtUtc").HasColumnType("datetime2");e.Property<DateTime?>("DropoffAtUtc").HasColumnType("datetime2");
            e.Property<bool?>("StopsIllustrative").HasColumnType("bit");
        });
    }
}
