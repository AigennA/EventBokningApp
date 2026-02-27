using EventBokningApp.Data;
using EventBokningApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EventBokningApp.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly AppDbContext _context;

    public VenueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Venue>> GetAllAsync()
        => await _context.Venues
            .Include(v => v.Events)
            .OrderBy(v => v.Name)
            .ToListAsync();

    public async Task<Venue?> GetByIdAsync(int id)
        => await _context.Venues
            .Include(v => v.Events)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task AddAsync(Venue venue)
        => await _context.Venues.AddAsync(venue);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<bool> ExistsAsync(int id)
        => await _context.Venues.AnyAsync(v => v.Id == id);

    public void Remove(Venue venue)
        => _context.Venues.Remove(venue);
}
