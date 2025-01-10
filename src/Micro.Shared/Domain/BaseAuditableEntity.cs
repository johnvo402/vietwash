using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Micro.Shared.Domain;
public abstract class BaseAuditableEntity : Entity
{
    public DateTimeOffset? CreatedAt { get; set; } = default;
    public string? CreatedBy { get; set; } = default;
    public DateTimeOffset? UpdatedAt { get; set; } = default;
    public string? UpdatedBy { get; set; } = default;
}