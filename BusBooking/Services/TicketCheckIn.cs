using BusBooking.Data;
using BusBooking.Models;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace BusBooking.Services;

public class TicketCheckIn(AppDbContext db,DbGate gate) {
    public const string Prefix="BUSCENTER-CHECKIN:";

    public static string Image(Guid ticketId) {
        var png=PngByteQRCodeHelper.GetQRCode(Prefix+ticketId.ToString("N"),QRCodeGenerator.ECCLevel.M,6);
        return "data:image/png;base64,"+Convert.ToBase64String(png);
    }

    public static bool MayCheckIn(Trip trip,string actor,bool admin,bool driver)=>
        admin||(driver?(trip.DriverId==actor||trip.CoDriverId==actor):trip.StaffId==actor);

    public static bool IsBoarding(Trip trip)=>trip.Status is TripStatus.Boarding or TripStatus.InProgress;

    public async Task<string> Record(int tripId,string? code,string actor,bool admin,bool driver,CancellationToken ct) {
        var value=code?.Trim()??"";
        if(value.StartsWith(Prefix,StringComparison.OrdinalIgnoreCase))value=value[Prefix.Length..];
        Guard.Input(Guid.TryParseExact(value,"N",out var ticketId)&&ticketId!=Guid.Empty,"Mã check-in không hợp lệ.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var trip=await gate.Trip(tripId,ct);
        if(!MayCheckIn(trip,actor,admin,driver))throw new BusinessException(403,"Bạn chưa được phân công check-in chuyến này.");
        var ticket=await db.Tickets.Include(t=>t.Seat).Include(t=>t.Booking)
            .SingleOrDefaultAsync(t=>t.Id==ticketId&&t.Seat.TripId==tripId,ct);
        if(ticket is null)throw new BusinessException(404,"Không tìm thấy vé thuộc chuyến này.");
        Guard.Rule(ticket.IsActive&&ticket.Booking.Status==BookingStatus.Active,"Vé không còn hiệu lực.");
        Guard.Rule(ticket.Booking.PaymentExpiresUtc==null,"Vé chưa hoàn tất thanh toán QR.");
        if(ticket.CheckedInUtc is DateTime previous){
            await tx.CommitAsync(ct);
            return $"Ghế {ticket.Seat.Code} đã check-in lúc {BusinessTime.Text(previous)}.";
        }
        Guard.Rule(IsBoarding(trip),"Chỉ check-in khi chuyến đang chuẩn bị chạy hoặc đang chạy.");
        var now=await gate.Now(ct);
        ticket.CheckedInUtc=now;
        gate.Audit(actor,"TicketCheckedIn",ticket.Id.ToString(),now);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return $"Đã check-in ghế {ticket.Seat.Code} lúc {BusinessTime.Text(now)}.";
    }
}
