using AuthService.Application.Interfaces;
using AuthService.Domain.Roles;
using AuthService.Domain.UserRoles;
using AuthService.Infrastructure.Persistence;
using Micro.Shared.QueryServices;
using Micro.Shared.Repository;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class RoleRepo : Repository<AuthDbContext, Role, Guid>, IRoleRepo
{
    public RoleRepo(AuthDbContext context, System.Data.IDbConnection dbConnection, IDapperQueryBuilder dapperQueryBuilder) : base(context, dbConnection, dapperQueryBuilder)
    {
    }

    public async ValueTask<bool> AddRoleToUser(Guid userId, List<string> roleNames, CancellationToken cancellationToken)
    {
        // Fetch roles into memory to avoid issues with translation
        var roles = await _context.Role
            .Where(r => roleNames.Contains(r.RoleName)) // Use the actual property of RoleName
            .Select(r => new { r.RoleName, r.Id }) // Project both RoleName and Id
            .ToListAsync(cancellationToken);

        // Make sure the roles are valid and exist
        if (roles.Count != roleNames.Count)
        {
            return false; // Some roles might not exist
        }

        // Map the roles to UserRole
        var userRoles = roles.Select(role => new UserRole
        {
            UserId = userId,
            RoleId = role.Id
        }).ToList();

        // Add the roles to the UserRoles table
        await _context.UserRole.AddRangeAsync(userRoles, cancellationToken);

        // Save changes and return success/failure based on result
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }



    public Task<List<Role>> GetRolesByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var query = from userRole in _context.UserRole
                    join role in _context.Role on userRole.RoleId equals role.Id
                    where userRole.UserId == userId
                    select role;
        return query.ToListAsync(cancellationToken);
    }
}