using Application.Feature.Common.Projections.Units;
using Microsoft.AspNetCore.Http;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceModel
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string CategoryId { get; set; } = default!;
        public List<UnitRelationModel> UnitRelations { get; set; } = [];

    }
}
