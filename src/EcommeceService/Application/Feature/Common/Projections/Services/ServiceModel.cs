using Application.Feature.Common.Projections.Units;
using Microsoft.AspNetCore.Http;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceModel
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
        public Ulid CategoryId { get; set; } = default!;
        public List<UnitRelationModel> UnitRelation { get; set; } = [];

    }
}
