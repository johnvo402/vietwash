using System.Data.Common;
using Application.Common.Interfaces.Registers;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces.Services.Identity;

public interface IUserManagerService : IScope
{

    public DbSet<Role> Roles { get; }


    Task CreateUserAsync(
        User user,
        DbTransaction? transaction = null
    );

    Task UpdateUserAsync(
        User user,
        DbTransaction? transaction = null
    );

    Task<Role> GetRolesInUser(Ulid userId);

    Task<bool> HasRolesInUserAsync(Ulid id, string roleNames);
}
