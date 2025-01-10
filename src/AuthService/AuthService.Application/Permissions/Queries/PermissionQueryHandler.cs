using AuthService.Application.Interfaces;
using AuthService.Domain.Permissions;
using ErrorOr;
using MediatR;
using Micro.Shared.Application.Security.Request;
using Micro.Shared.Model;

namespace AuthService.Application.Permissions.Queries;
public record PermissionQueryHandler(IPermissionRepo _permissionRepo) : IRequestHandler<PermissionQuery, ErrorOr<IEnumerable<Permission>?>>
{
    public async Task<ErrorOr<IEnumerable<Permission>?>> Handle(PermissionQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _permissionRepo.GetAllAsync(request.QueryParameters);
        return permissions.ToList();
    }
}
