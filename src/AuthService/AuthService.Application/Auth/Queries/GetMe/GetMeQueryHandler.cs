using MediatR;
using Micro.Shared.Infrastructure.CurrentUserProvider;
using ErrorOr;
using AuthService.Application.Interfaces;

namespace AuthService.Application.Auth.Queries.GetMe;

public class GetMeQueryHandler(ICurrentUser _currentUser, IUserRepo userRepo, IRoleRepo roleRepo, IPermissionRepo permissionRepo) : IRequestHandler<GetMeQuery, ErrorOr<GetMeResponse>>
{

    public async Task<ErrorOr<GetMeResponse>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepo.GetByIDAsync(Guid.Parse(_currentUser.Id));
        if (user == null)
        {
            return Error.NotFound("backend.not_found", "User not found");
        }

        var roles = await roleRepo.GetRolesByUserId(user.Id, cancellationToken);
        var permissions = permissionRepo.GetPermissionsByRoleIds(roles.Select(s => s.Id).ToList(), cancellationToken).Result.Select(x => x.PermissionKey).ToList();

        var response = new GetMeResponse
        {
            UserId = user.Id.ToString(),
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = roles.Select(r => r.RoleName).ToArray(),
            Permission = permissions.ToArray(),
            PhoneNumber = user.PhoneNumber,
            Avatar = user.Avatar
        };

        return response;
    }
}