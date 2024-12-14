using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IUserActivity
{
    Task<UserActivity?> GetByUserIdAsync(string userId);
    Task AddAsync(UserActivity userActivity);
    
}