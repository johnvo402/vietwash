using ErrorOr;
using Micro.Shared.Application.Security.Request;

namespace Micro.Shared.Application.Interface;
public interface IAuthorizationService
{
    ErrorOr<Success> AuthorizeCurrentUser<T>(
        IAuthorizeableRequest<T> request,
        List<string> requiredRoles,
        List<string> requiredPermissions,
        List<string> requiredPolicies);
}