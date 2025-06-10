using System.Data;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Accounts;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services.Identity;

public class UserManagerService(IDbContext context) : IUserManagerService
{

    private readonly DbSet<Account> userContext = context.Set<Account>();
    public DbSet<Account> Accounts => userContext;

}
