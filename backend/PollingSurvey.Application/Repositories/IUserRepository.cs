using PollingSurvey.Domain.Entities;

namespace PollingSurvey.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByUsernameAsync(string username);
    Task AddUserAsync(User user);
    Task SaveChangesAsync();
}
