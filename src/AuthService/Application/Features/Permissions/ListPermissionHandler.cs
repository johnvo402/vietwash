using Application.Common.Interfaces.Services.Identity;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Permissions;

public class ListPermissionHandler(IRoleManagerService roleManagerService)
    : IRequestHandler<ListPermissionQuery, IEnumerable<ListPermissionResponse>>
{
    public async ValueTask<IEnumerable<ListPermissionResponse>> Handle(
        ListPermissionQuery request,
        CancellationToken cancellationToken
    )
    {
        return await roleManagerService
     .RoleClaims
     .Where(x => x.ClaimType == "permission")
     .GroupBy(x => x.ClaimValue)
     .Select(g => new ListPermissionResponse  
     {
         ClaimType = g.First().ClaimType,
         ClaimValue = g.Key 
     })
     .ToListAsync(cancellationToken);
    }
}
