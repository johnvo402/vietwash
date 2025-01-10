using ErrorOr;
using Micro.Shared.Application.Security.Policy;
using Micro.Shared.Application.Security.Request;
using Micro.Shared.Data;
using Micro.Shared.Infrastructure.CurrentUserProvider;


namespace Micro.Shared.Infrastructure.Security.PolicyEnforcer
{
    public class PolicyEnforcer : IPolicyEnforcer
    {
        public ErrorOr<Success> Authorize(
            ICurrentUser currentUser,
            string policy)
        {

            switch (policy)
            {
                case Policy.SelfOrAdmin:
                    return SelfOrAdminPolicy(currentUser);
                default:
                    return Error.Unexpected(description: "Unknown policy name");
            }
        }
        private static ErrorOr<Success> SelfOrAdminPolicy(ICurrentUser currentUser) =>
           currentUser.Roles.Contains(RoleName.Admin) || currentUser.Roles.Contains(RoleName.Staff)
               ? Result.Success
               : Error.Unauthorized(description: "Requesting user failed policy requirement");

    }
}
