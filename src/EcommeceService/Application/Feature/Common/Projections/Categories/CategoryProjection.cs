using Domain.Aggregates.Enums;
using Shared.Kernel.Common;

namespace Application.Feature.Common.Projections.Categories;

public class CategoryProjection : BaseEntity<string>
{
    public string? Name { get; set; }
    public string? Path { get; set; }
    public string? ParentId { get; set; }
    public ActivationStatus Status { get; set; } = default!;
}
