using System.Data;
using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
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

    public async Task CreateUserAsync(User user, DbTransaction? transaction = null)
    {
        var isOwnerTransaction = transaction != null;
        try
        {
            if (!isOwnerTransaction && context.DatabaseFacade.CurrentTransaction == null)
                await context.DatabaseFacade.BeginTransactionAsync();

            await userContext.AddAsync(user);

            await context.SaveChangesAsync();

            if (!isOwnerTransaction)
                await context.DatabaseFacade.CommitTransactionAsync();
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
            user.DequeueUncommittedEvents();


            if (!isOwnerTransaction)
                await context.DatabaseFacade.RollbackTransactionAsync();

            throw;
        }
    }

    public async Task UpdateUserAsync(User user, DbTransaction? transaction = null)
    {
        var isOwnerTransaction = transaction != null;
        try
        {
            if (!isOwnerTransaction && context.DatabaseFacade.CurrentTransaction == null)
                await context.DatabaseFacade.BeginTransactionAsync();

            userContext.Update(user);
            await context.SaveChangesAsync();

            if (!isOwnerTransaction)
                await context.DatabaseFacade.CommitTransactionAsync();
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

            if (!isOwnerTransaction)
                await context.DatabaseFacade.RollbackTransactionAsync();

            throw;
        }
    }

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
