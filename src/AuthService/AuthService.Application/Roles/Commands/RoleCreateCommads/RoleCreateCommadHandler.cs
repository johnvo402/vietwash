using AuthService.Application.Interfaces;
using AuthService.Domain.Roles;
using ErrorOr;
using MediatR;
using Micro.Shared.Application.Security.Request;
using Micro.Shared.Infrastructure.CurrentUserProvider;
using Micro.Shared.Model;

namespace AuthService.Application.Roles.Commands.RoleCreateCommads;
public record RoleCreateCommandHandler(IRoleRepo _roleRepo, ICurrentUser _currentUser) : IRequestHandler<RoleCreateCommand, ErrorOr<string>>
{
    public async Task<ErrorOr<string>> Handle(RoleCreateCommand request, CancellationToken cancellationToken)
    {
        var roleCheck = await _roleRepo.GetAllAsync(new QueryParameters { Where = $"role_name = '{request.Name}'" });
        if (roleCheck.Any())
        {
            return Error.Conflict("backend.roles.already_exist");
        }
        var role = new Role
        {
            RoleName = request.Name,
            OrgId = _currentUser.OrgId

        };
        var result = await _roleRepo.CreateAsync(role, cancellationToken);
        if (result == null)
        {
            return Error.Failure("backend.created.failed");
        }
        return "backend.created.success";
    }
}
