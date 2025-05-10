using System.Data;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services.Identity;

public class UserManagerService(IDbContext context) : IUserManagerService
{

    private readonly DbSet<User> userContext = context.Set<User>();
    public DbSet<User> Users => userContext;
   
}
