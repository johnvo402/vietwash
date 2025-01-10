using System.Data;
using AuthService.Application.Interfaces;
using AuthService.Domain.Permissions;
using AuthService.Infrastructure.Persistence;
using Micro.Shared.QueryServices;
using Micro.Shared.Repository;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class PermissionRepo : Repository<AuthDbContext, Permission, Guid>, IPermissionRepo
{
    public PermissionRepo(AuthDbContext context, IDbConnection dbConnection, IDapperQueryBuilder dapperQueryBuilder) : base(context, dbConnection, dapperQueryBuilder)
    {
    }

    public async Task<List<Permission>> GetPermissionsByRoleIds(List<Guid> roleIds, CancellationToken cancellationToken)
    {
        var query = from rolePermission in _context.RolePermission
                    join permission in _context.Permission on rolePermission.PermissionId equals permission.Id
                    where roleIds.Contains(rolePermission.RoleId)
                    select permission;
        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }
}