using Application.Common.Interfaces.Services.Identity;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Permissions;

public class ListPermissionHandler(IRoleManagerService roleManager)
    : IRequestHandler<ListPermissionQuery, IEnumerable<ListPermissionResponse>>
{
    public async ValueTask<IEnumerable<ListPermissionResponse>> Handle(
        ListPermissionQuery request,
        CancellationToken cancellationToken
    )
    {
        return await roleManager.Permissions.Select(x => new ListPermissionResponse
            {
                Key = x.Key,
                Description = x.Description
            }).ToListAsync();

    }
}
