using AuthService.Domain.Users.Entity;
using Micro.Shared.Repository;

namespace AuthService.Application.Interfaces;

public interface IUserRepo : IRepository<User, Guid>
{
    ValueTask<User?> GetUserByEmail(string Email, CancellationToken cancellationToken);
}