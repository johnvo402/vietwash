using Domain.Aggregates.Enums;

namespace Application.Feature.Common.Projections.Services
{
    public class CategoryModel
    {
        public string? Name { get; set; }
        public long? ParentId { get; set; }

        public ActivationStatus Status { get; set; } = ActivationStatus.Active;
    }
}
