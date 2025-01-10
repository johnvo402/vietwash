using System.Data;
using AuthService.Application.Interfaces;
using AuthService.Domain.UserActivities;
using AuthService.Infrastructure.Persistence;
using Micro.Shared.QueryServices;
using Micro.Shared.Repository;

namespace AuthService.Application.Auth.Commands.Login
{
    public class UserActivityRepo : Repository<AuthDbContext, UserActivity, Guid>, IUserActivityRepo
    {
        public UserActivityRepo(AuthDbContext context, IDbConnection dbConnection, IDapperQueryBuilder dapperQueryBuilder) : base(context, dbConnection, dapperQueryBuilder)
        {
        }
    }
}
