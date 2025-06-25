using Domain.Aggregates.Services.Enums;

namespace Application.Feature.Common.Projections.Services
{
    public class CategoryModel
    {
        public string? Name { get; set; }
        public string? ParentId { get; set; }

        public ActivationStatus Status { get; set; } = ActivationStatus.active;
    }
}
