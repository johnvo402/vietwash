using Contracts.Dtos.Models;
using JohnChum.SharedKernel.Application.Common;

namespace Application.Features.Common.Projections.Roles;

public class RoleClaimDetailProjection : DefaultBaseResponse
{
    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }
}
