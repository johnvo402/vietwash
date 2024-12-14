namespace Micro.Shared.Common;
public abstract class BaseAuditableEntity<T> : BaseEntity<T>
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }
}