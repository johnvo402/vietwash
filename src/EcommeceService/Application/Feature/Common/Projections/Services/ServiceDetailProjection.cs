using Application.Feature.Common.Mapping.Categories;
using Application.Feature.Common.Mapping.Units;
using Application.Feature.Common.Projections.Categories;
using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Services;


namespace Application.Feature.Common.Projections.Services
{
    public class ServiceDetailProjection : ServiceProjection
    {
        public CategoryProjection Category { get; set; } = default!;
        public string? Description { get; set; }
        public long BranchId { get; set; } = default!;
		public double AverageRating { get; set; }
		public ICollection<UnitRelationProjection> UnitRelations { get; set; } = [];

        public override void MappingFrom(Service service)
        {
            base.MappingFrom(service);
            Category = service.Category.ToCategoryProjectionResponse();
            Description = service.Description;
            BranchId = service.BranchId;
            UnitRelations = service
                .UnitRelations.Select(x => x.ToUnitRelationProjectionResponse())
                .ToList();
        }
    }
}
