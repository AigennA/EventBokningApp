using EventBokningApp.Data;
using EventBokningApp.DTOs;
using EventBokningApp.Exceptions;
using EventBokningApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EventBokningApp.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Booking> CreateBookingAsync(CreateBookingDto dto)
    {
        var ticket = await _context.Tickets.FindAsync(dto.TicketId)
            ?? throw new NotFoundException($"Ticket with id {dto.TicketId} not found.");

        if (ticket.QuantityAvailable < dto.Quantity)
            throw new BusinessException(
                $"Not enough tickets available. Requested: {dto.Quantity}, Available: {ticket.QuantityAvailable}.");

        var booking = new Booking
        {
            TicketId = dto.TicketId,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            Quantity = dto.Quantity,
            TotalPrice = dto.Quantity * ticket.Price,
            BookingDate = DateTime.UtcNow,
            IsCancelled = false
        };

        ticket.QuantityAvailable -= dto.Quantity;

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return booking;
    }

    public async Task<bool> CancelBookingAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Ticket)
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new NotFoundException($"Booking with id {bookingId} not found.");

        if (booking.IsCancelled)
            throw new BusinessException("Booking is already cancelled.");

        booking.IsCancelled = true;
        booking.Ticket.QuantityAvailable += booking.Quantity;

        await _context.SaveChangesAsync();
        return true;
    }
}
