using System.Data;
using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services.Identity;

public class UserManagerService(IDbContext context) : IUserManagerService
{
    private readonly DbSet<Role> roleContext = context.Set<Role>();
    public DbSet<Role> Roles => roleContext;

    private readonly DbSet<User> userContext = context.Set<User>();
    public DbSet<User> Users => userContext;
    public async Task<Role> GetRolesInUser(Ulid userId) =>
        await userContext
            .Where(u => u.Id == userId)
            .Select(u => new Role
            {
                Id = u.Role.Id,
                Name = u.Role.Name,
                RolePermissions = u.Role.RolePermissions!.ToList()
            })
            .FirstAsync();

    public async Task<bool> HasRolesInUserAsync(Ulid id, string roleNames) =>
        await userContext.AnyAsync(x => x.Id == id && x.Role!.Name == roleNames);
}
