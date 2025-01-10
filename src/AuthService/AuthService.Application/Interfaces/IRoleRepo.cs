using AuthService.Domain.Roles;
using AuthService.Domain.UserRoles;
using Micro.Shared.Repository;

namespace AuthService.Application.Interfaces;

public interface IRoleRepo : IRepository<Role, Guid>
{
    Task<List<Role>> GetRolesByUserId(Guid userId, CancellationToken cancellationToken);
    ValueTask<bool> AddRoleToUser(Guid userId, List<string> roleName, CancellationToken cancellationToken);
}