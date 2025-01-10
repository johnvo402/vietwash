using System.Data;
using AuthService.Application.Interfaces;
using AuthService.Domain.Users.Entity;
using AuthService.Infrastructure.Persistence;
using Micro.Shared.QueryServices;
using Micro.Shared.Repository;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Users.Repositories;
public class UserRepo : Repository<AuthDbContext, User, Guid>, IUserRepo
{
    public UserRepo(AuthDbContext context, IDbConnection dbConnection, IDapperQueryBuilder dapperQueryBuilder) : base(context, dbConnection, dapperQueryBuilder)
    {
    }

    public async ValueTask<User?> GetUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}