using EventBokningApp.Data;
using EventBokningApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EventBokningApp.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(int id)
        => await _context.Bookings
            .Include(b => b.Ticket)
            .ThenInclude(t => t.Event)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IEnumerable<Booking>> GetAllAsync()
        => await _context.Bookings
            .Include(b => b.Ticket)
            .ThenInclude(t => t.Event)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

    public async Task<IEnumerable<Booking>> GetByEventIdAsync(int eventId)
        => await _context.Bookings
            .Include(b => b.Ticket)
            .ThenInclude(t => t.Event)
            .Where(b => b.Ticket.EventId == eventId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

    public async Task<IEnumerable<Booking>> GetByEmailAsync(string email)
        => await _context.Bookings
            .Include(b => b.Ticket)
            .ThenInclude(t => t.Event)
            .ThenInclude(e => e.Venue)
            .Where(b => b.CustomerEmail == email && !b.IsCancelled)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

    public async Task AddAsync(Booking booking)
        => await _context.Bookings.AddAsync(booking);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
