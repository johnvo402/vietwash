using System.Data;
using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Ardalis.GuardClauses;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services.Identity;

public class UserManagerService(
    IDbContext context,
    ILogger logger
) : IUserManagerService
{

    private readonly DbSet<Role> roleContext = context.Set<Role>();
    public DbSet<Role> Roles => roleContext;

    private readonly DbSet<User> userContext = context.Set<User>();

    public async Task CreateUserAsync(
        User user,
        DbTransaction? transaction = null
    )
    {
        try
        {
            if (transaction == null)
            {
                await context.DatabaseFacade.BeginTransactionAsync();
            }
            else
            {
                await context.UseTransactionAsync(transaction);
            }

            if (transaction == null)
            {
                await context.DatabaseFacade.CommitTransactionAsync();
            }
        }
        catch (Exception ex)
        {
            logger.Error(
                ex,
                "Error in method {MethodName}. Exception Type: {ExceptionType}. Error Message: {ErrorMessage}. StackTrace: {StackTrace}",
                nameof(CreateUserAsync),
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace
            );

            if (transaction == null)
            {
                await context.DatabaseFacade.RollbackTransactionAsync();
            }
            throw;
        }
    }

    public async Task UpdateUserAsync(
        User user,
        DbTransaction? transaction = null
    )
    {
        try
        {
            if (transaction == null)
            {
                await context.DatabaseFacade.BeginTransactionAsync();
            }
            else
            {
                await context.UseTransactionAsync(transaction);
            }

            if (transaction == null)
            {
                await context.DatabaseFacade.CommitTransactionAsync();
            }
        }
        catch (Exception ex)
        {
            logger.Error(
                ex,
                "Error in method {MethodName}. Exception Type: {ExceptionType}. Error Message: {ErrorMessage}. StackTrace: {StackTrace}",
                nameof(UpdateUserAsync),
                ex.GetType().Name,
                ex.Message,
                ex.StackTrace
            );

            if (transaction == null)
            {
                await context.DatabaseFacade.RollbackTransactionAsync();
            }
            throw;
        }
    }



    public async Task<Role> GetRolesInUser(Ulid userId) =>
        (await userContext.Where(u => u.Id == userId).Select(u => new Role
        {
            Id = u.Role.Id,
            Name = u.Role.Name,
            RoleClaims = u.Role.RoleClaims!
                .Where(rc => rc.ClaimType == "permission")
                .Select(rc => new RoleClaim
                {
                    ClaimType = rc.ClaimType,
                    ClaimValue = rc.ClaimValue
                })
                .ToList()
        }).FirstAsync());



    public async Task<bool> HasRolesInUserAsync(Ulid id, string roleNames) =>
        await userContext.AnyAsync(x =>
            x.Id == id && x.Role!.Name == roleNames
        );

}
