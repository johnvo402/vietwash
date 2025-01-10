using AuthService.Application.Interfaces;
using AuthService.Domain.Roles;
using ErrorOr;
using MediatR;
using Micro.Shared.Application.Security.Request;
using Micro.Shared.Model;

namespace AuthService.Application.Roles.Queries;
public record RoleQueryHandler(IRoleRepo RoleRepo) : IRequestHandler<RoleQuery, ErrorOr<List<Role>>>
{
    public async Task<ErrorOr<List<Role>>> Handle(RoleQuery request, CancellationToken cancellationToken)
    {
        var roles = await RoleRepo.GetAllAsync(request.QueryParameters);
        return roles.ToList();
    }
}
