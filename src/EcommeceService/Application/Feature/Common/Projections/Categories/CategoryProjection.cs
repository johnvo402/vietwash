using Application.Feature.Common.Projections.Services;
using Domain.Aggregates.Enums;
using Shared.Kernel.Common;

namespace Application.Feature.Common.Projections.Categories;

public class CategoryProjection : BaseEntity<long>
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Path { get; set; }
    public long? ParentId { get; set; }
    public ActivationStatus Status { get; set; } = default!;
}

public class CategoryServiceProjection : BaseEntity<long>
{
    public string? Name { get; set; }
    public ICollection<ServiceProjection> Services { get; set; } = [];
}
