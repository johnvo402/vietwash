using ErrorOr;
using Micro.Shared.Application.Interface;
using Micro.Shared.Application.Security.Request;
using Micro.Shared.Infrastructure.CurrentUserProvider;
using Micro.Shared.Infrastructure.Security.PolicyEnforcer;


namespace Micro.Shared.Infrastructure.Security
{
    public class AuthorizationService(
    IPolicyEnforcer _policyEnforcer,
    ICurrentUser currentUser)
        : IAuthorizationService
    {
        public ErrorOr<Success> AuthorizeCurrentUser<T>(
            IAuthorizeableRequest<T> request,
            List<string> requiredRoles,
            List<string> requiredPermissions,
            List<string> requiredPolicies)
        {

            if (requiredPermissions.Except(currentUser.Permissions).Any())
            {
                return Error.Unauthorized(description: "User is missing required permissions for taking this action");
            }

            if (requiredRoles.Except(currentUser.Roles).Any())
            {
                return Error.Unauthorized(description: "User is missing required roles for taking this action");
            }

            foreach (var policy in requiredPolicies)
            {
                var authorizationAgainstPolicyResult = _policyEnforcer.Authorize(currentUser, policy);

                if (authorizationAgainstPolicyResult.IsError)
                {
                    return authorizationAgainstPolicyResult.Errors;
                }
            }

            return Result.Success;
        }
    }
}
