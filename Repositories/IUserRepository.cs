using EventBokningApp.Models;

namespace EventBokningApp.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
