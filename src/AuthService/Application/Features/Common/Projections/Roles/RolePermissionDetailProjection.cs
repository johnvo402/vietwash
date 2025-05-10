using Contracts.Dtos.Models;
using JohnChum.SharedKernel.Application.Common;

namespace Application.Features.Common.Projections.Roles;

public class RolePermissionDetailProjection : DefaultBaseResponse
{
    public Ulid PermissionId { get; set; }
    public PermissionModel? Permission { get; set; }
}
