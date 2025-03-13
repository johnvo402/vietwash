using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Roles
{
    public class Permission : DefaultEntity
    {
        public string Key { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<RolePermission>? RolePermissions { get; set; } = [];
    }
}
