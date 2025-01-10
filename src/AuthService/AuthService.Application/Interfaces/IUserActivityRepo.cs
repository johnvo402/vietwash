
using AuthService.Domain.UserActivities;
using Micro.Shared.Repository;

namespace AuthService.Application.Interfaces;

public interface IUserActivityRepo : IRepository<UserActivity, Guid>
{

}