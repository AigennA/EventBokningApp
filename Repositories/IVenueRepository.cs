using EventBokningApp.Models;

namespace EventBokningApp.Repositories;

public interface IVenueRepository
{
    Task<IEnumerable<Venue>> GetAllAsync();
    Task<Venue?> GetByIdAsync(int id);
    Task AddAsync(Venue venue);
    Task SaveChangesAsync();
    Task<bool> ExistsAsync(int id);
    void Remove(Venue venue);
}
