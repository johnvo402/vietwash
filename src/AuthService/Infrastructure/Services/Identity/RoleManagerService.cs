using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Ardalis.GuardClauses;
using Domain.Aggregates.Roles;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Identity;

public class RoleManagerService(IDbContext context) : IRoleManagerService
{
    private readonly DbSet<Role> roleContext = context.Set<Role>();
    public DbSet<Role> Roles => roleContext;

    private readonly DbSet<RolePermission> roleClaimContext = context.Set<RolePermission>();
    public DbSet<RolePermission> RoleClaims => roleClaimContext;

    private readonly DbSet<Permission> permissionContext = context.Set<Permission>();

    public DbSet<Permission> Permissions => permissionContext;

    private const string NOT_FOUND_MESSAGE = $"{nameof(Role)} is not found";

    public async Task DeleteRoleAsync(Role role)
    {
        roleContext.Remove(role);
        await context.SaveChangesAsync();
    }

    public async Task<Role> CreateRoleAsync(Role role)
    {
        await roleContext.AddAsync(role);
        await context.SaveChangesAsync();

        return role;
    }

    public async Task<IList<Role>> CreateRangeRoleAsync(IEnumerable<Role> roles)
    {
        await roleContext.AddRangeAsync(roles);
        await context.SaveChangesAsync();
        return [.. roles];
    }

    public async Task<Role> UpdateRoleAsync(Role role, IEnumerable<RolePermission>? roleClaims)
    {
        try
        {
            await context.DatabaseFacade.BeginTransactionAsync();

            roleContext.Update(role);
            await context.SaveChangesAsync();

            if (roleClaims?.Any() == true)
            {
                await UpdateRoleClaimAsync(roleClaims, role);
            }
            else
            {
                List<RolePermission> claimsToDelete = await roleClaimContext
                    .Where(x => x.RoleId == role.Id)
                    .ToListAsync();
                roleClaimContext.RemoveRange(claimsToDelete);
                await context.SaveChangesAsync();
            }

            await context.DatabaseFacade.CommitTransactionAsync();
            return role;
        }
        catch (Exception)
        {
            await context.DatabaseFacade.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Role?> GetByIdAsync(Ulid id) =>
        await roleContext.Where(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Role?> FindByIdAsync(Ulid id) =>
        await roleContext.Where(x => x.Id == id).Include(x => x.RolePermissions)!.ThenInclude(x => x.Permission).FirstOrDefaultAsync();

    public async Task<Role?> FindByNameAsync(string name) =>
        await roleContext
            .Where(x => x.Name == name)
            .Include(x => x.RolePermissions)!.ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync();

    public async Task<List<Role>> ListAsync() => await roleContext.ToListAsync();

    public async Task UpdateRoleClaimAsync(IEnumerable<RolePermission> roleClaims, Role role)
    {
        Role currentRole = Guard.Against.NotFound(
            $"{role.Id}",
            await roleContext
                .Where(x => x.Id == role.Id)
                .Include(x => x.RolePermissions)!.ThenInclude(x => x.Permission)
                .FirstOrDefaultAsync(),
            NOT_FOUND_MESSAGE
        );
        Guard.Against.Null(roleClaims, nameof(roleClaims), $"{nameof(roleClaims)} is not null");

        IEnumerable<RolePermission> rolesClaimsToProcess = roleClaims;
        ICollection<RolePermission> currentRoleClaims = currentRole.RolePermissions!;

        IEnumerable<RolePermission> roleClaimsToInsert = rolesClaimsToProcess.Where(x =>
            !currentRoleClaims.Any(p => p.Id == x.Id)
        );
        IEnumerable<RolePermission> roleClaimsToModify = currentRoleClaims.Where(x =>
            rolesClaimsToProcess.Any(p => p.Id == x.Id)
        );
        IEnumerable<RolePermission> roleClaimsToRemove = currentRoleClaims.Where(x =>
            !rolesClaimsToProcess.Any(p => p.Id == x.Id)
        );

        //IEnumerable<UserClaim> userClaims = ProcessUserClaimUpdate(
        //    roleClaimsToModify,
        //    rolesClaimsToProcess
        //);

        // remove
        await RemoveClaimsFromRoleAsync(
            role,
            [
                .. roleClaimsToRemove.Select(x => x.PermissionId),
            ]
        );

        //update
        roleClaimContext.UpdateRange(roleClaimsToModify);
        await context.SaveChangesAsync();

        var keyValuePairClaims = roleClaimsToInsert.Select(x => x.PermissionId).ToList();
        // insert
        await AddClaimsToRoleAsync(role, keyValuePairClaims);
    }

    public async Task AddClaimsToRoleAsync(
        Role role,
        IEnumerable<Ulid> claims
    )
    {
        Role currentRole = Guard.Against.NotFound(
            $"{role.Id}",
            await roleContext
                .Where(x => x.Id == role.Id)
                .Include(x => x.RolePermissions)!
                .AsSplitQuery()
                .FirstOrDefaultAsync(),
            NOT_FOUND_MESSAGE
        );
        ICollection<RolePermission> currentRoleClaims = currentRole.RolePermissions!;
        IEnumerable<Ulid> roleClaimsToProcess = claims;

        if (
            roleClaimsToProcess.Any(x =>
                currentRoleClaims.Any(p => p.PermissionId == x)
            )
        )
        {
            throw new Exception($"1 or more elements of {nameof(claims)} exists in role claims");
        }

        List<RolePermission> roleClaimsToInsert =
        [
            .. roleClaimsToProcess.Select(x => new RolePermission
            {   
                 PermissionId = x,
                RoleId = currentRole.Id,
            }),
        ];


        await roleClaimContext.AddRangeAsync(roleClaimsToInsert);
        await context.SaveChangesAsync();
    }

    public async Task RemoveClaimsFromRoleAsync(
        Role role,
        IEnumerable<Ulid> permission
    )
    {
        if (!permission.Any())
        {
            return;
        }

        Role currentRole = Guard.Against.NotFound(
            $"{role.Id}",
            await FindByIdAsync(role.Id),
            NOT_FOUND_MESSAGE
        );

        ICollection<RolePermission> currentRoleClaims = currentRole.RolePermissions!;
        if (
            permission.Any(x =>
                !currentRoleClaims.Any(p => p.Id == x)
            )
        )
        {
            throw new Exception("One or many claims is not existed in role.");
        }

        IEnumerable<RolePermission> claimsToDelete = currentRoleClaims.Where(x =>
            permission.Any(p => p == x.PermissionId)
        );

        roleClaimContext.RemoveRange(claimsToDelete);
        await context.SaveChangesAsync();
    }

    public Task<List<RolePermission>> GetClaimsByRoleAsync(Ulid roleId) =>
        GetClaimsByRolesAsync([roleId]);

    public async Task<List<RolePermission>> GetClaimsByRolesAsync(IEnumerable<Ulid> roleIds) =>
        await roleClaimContext.Where(x => roleIds.Contains(x.RoleId)).ToListAsync();

    public async Task<bool> HasClaimInRoleAsync(Ulid roleId, Ulid claimId) =>
        await roleContext.AnyAsync(x => x.Id == roleId && x.RolePermissions!.Any(p => p.PermissionId == claimId));

    public async Task<bool> HasClaimInRoleAsync(
        Ulid roleId,
        IEnumerable<Ulid> claims
    )
    {
        var roleClaims = await roleContext
            .Where(x => x.Id == roleId)
            .SelectMany(x => x.RolePermissions!)
            .ToListAsync();

        return roleClaims.Any(x => claims.Contains(x.PermissionId));
    }

    private async Task<Role> GetAsync(Ulid id) =>
        Guard.Against.NotFound($"{id}", await GetByIdAsync(id), NOT_FOUND_MESSAGE);

    //private static IEnumerable<UserClaim> ProcessUserClaimUpdate(
    //    IEnumerable<RoleClaim> roleClaimsToModify,
    //    IEnumerable<RoleClaim> rolesClaimsToProcess
    //)
    //{
    //    foreach (RoleClaim claim in roleClaimsToModify)
    //    {
    //        RoleClaim? correspondedClaim = rolesClaimsToProcess.FirstOrDefault(x =>
    //            x.Id == claim.Id
    //        );

    //        if (correspondedClaim == null)
    //        {
    //            continue;
    //        }

    //        claim.ClaimValue = correspondedClaim.ClaimValue;
    //        List<UserClaim> updatedUserClaims = claim.UpdateUserClaim();

    //        for (int i = 0; i < updatedUserClaims.Count; i++)
    //        {
    //            yield return updatedUserClaims[i];
    //        }
    //    }
    //}
}
