using AuthService.Domain.Permissions;
using Micro.Shared.Repository;

namespace AuthService.Application.Interfaces;

public interface IPermissionRepo : IRepository<Permission, Guid>
{
    Task<List<Permission>> GetPermissionsByRoleIds(List<Guid> roleIds, CancellationToken cancellationToken);
}