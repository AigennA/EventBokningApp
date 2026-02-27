using EventBokningApp.Models;

namespace EventBokningApp.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id);
    Task<IEnumerable<Booking>> GetAllAsync();
    Task<IEnumerable<Booking>> GetByEventIdAsync(int eventId);
    Task<IEnumerable<Booking>> GetByEmailAsync(string email);
    Task AddAsync(Booking booking);
    Task SaveChangesAsync();
}
