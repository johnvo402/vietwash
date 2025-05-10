using JohnChum.SharedKernel.Domain.Common;

namespace Application.Features.Common.Projections.Roles
{
    public class PermissionModel : DefaultEntity
    {
        public string? Description { get; set; }
        public string? Key { get; set; }
    }
}
