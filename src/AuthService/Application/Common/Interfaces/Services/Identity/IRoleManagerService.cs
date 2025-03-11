using Application.Common.Interfaces.Registers;
using Domain.Aggregates.Roles;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces.Services.Identity;

public interface IRoleManagerService : IScope
{
    public DbSet<Role> Roles { get; }

    public DbSet<RolePermission> RoleClaims { get; }

    public DbSet<Permission> Permissions {get;}

    Task<Role> CreateRoleAsync(Role role);

    Task<IList<Role>> CreateRangeRoleAsync(IEnumerable<Role> roles);

    Task<Role> UpdateRoleAsync(Role role, IEnumerable<RolePermission>? roleClaims);

    Task DeleteRoleAsync(Role role);

    Task<List<Role>> ListAsync();

    // get role only
    Task<Role?> GetByIdAsync(Ulid id);

    // ger role with claims
    Task<Role?> FindByIdAsync(Ulid id);

    Task<Role?> FindByNameAsync(string name);

    Task UpdateRoleClaimAsync(IEnumerable<RolePermission> roleClaims, Role role);

    Task AddClaimsToRoleAsync(Role role, IEnumerable<Ulid> claims);

    Task RemoveClaimsFromRoleAsync(Role role, IEnumerable<Ulid> roleClaims);

    Task<List<RolePermission>> GetClaimsByRoleAsync(Ulid roleId);

    Task<List<RolePermission>> GetClaimsByRolesAsync(IEnumerable<Ulid> roleIds);

    Task<bool> HasClaimInRoleAsync(Ulid roleId, Ulid permissionId);

    Task<bool> HasClaimInRoleAsync(Ulid roleId, IEnumerable<Ulid> permissionIds);
}
