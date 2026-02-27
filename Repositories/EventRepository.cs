using EventBokningApp.Data;
using EventBokningApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EventBokningApp.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Event>> GetAllAsync()
        => await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Tickets)
            .OrderBy(e => e.Date)
            .ToListAsync();

    public async Task<IEnumerable<Event>> GetUpcomingAsync()
        => await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Tickets)
            .Where(e => e.Date > DateTime.UtcNow)
            .OrderBy(e => e.Date)
            .ToListAsync();

    public async Task<Event?> GetByIdAsync(int id)
        => await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Tickets)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task AddAsync(Event ev)
        => await _context.Events.AddAsync(ev);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<bool> ExistsAsync(int id)
        => await _context.Events.AnyAsync(e => e.Id == id);

    public void Remove(Event ev)
        => _context.Events.Remove(ev);
}
