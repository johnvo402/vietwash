using AuthService.Application.Interfaces;
using AuthService.Domain.Permissions;
using ErrorOr;
using MediatR;

namespace AuthService.Application.Permissions.Commands.PermissionCreateCommands;
public class PermissionCreateCommandHandler(IPermissionRepo _permissionRepo) : IRequestHandler<PermissionCreateCommand, ErrorOr<string>>
{
    public async Task<ErrorOr<string>> Handle(PermissionCreateCommand request, CancellationToken cancellationToken)
    {
        var permission = new Permission(request.Key, request.Description);
        var check = await _permissionRepo.CreateAsync(permission, cancellationToken);
        if (check == null)
        {
            return Error.Failure("backend.created.failed");
        }
        return "backend.created.success";
    }
}
