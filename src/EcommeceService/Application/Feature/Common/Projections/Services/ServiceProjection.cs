using Application.Common.Security;
using Application.Feature.Common.Projections.Units;
using Application.Feature.Services.Queries.List;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Services;
using Shared.Kernel.Common;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceProjection : BaseEntity
    {
        public string Name { get; set; } = default!;
        public long? CategoryId { get; set; }
        public long BranchId { get; set; }

        [File]
        public string? Image { get; set; }
        public ActivationStatus? Status { get; set; }
        public CategoryService? Category { get; set; }
        public ICollection<UnitRelationProjection> UnitRelations { get; set; } = [];

        public virtual void MappingFrom(Service service)
        {
            Id = service.Id;
            PublicId = service.PublicId;
            CreatedAt = service.CreatedAt;
            CreatedBy = service.CreatedBy;
            UpdatedAt = service.UpdatedAt;
            UpdatedBy = service.UpdatedBy;

            Name = service.Name;
            Image = service.Image;
            Status = service.Status;
            CategoryId = service.CategoryId;
            BranchId = service.BranchId;
        }
    }
}
