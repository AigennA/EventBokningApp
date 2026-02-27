using EventBokningApp.Models;

namespace EventBokningApp.Repositories;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllAsync();
    Task<IEnumerable<Event>> GetUpcomingAsync();
    Task<Event?> GetByIdAsync(int id);
    Task AddAsync(Event ev);
    Task SaveChangesAsync();
    Task<bool> ExistsAsync(int id);
    void Remove(Event ev);
}
