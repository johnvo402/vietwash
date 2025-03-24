using System.Text.Json.Serialization;
using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Services.Enums;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceModel
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Image { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceStatus Status { get; set; } = default!;
        public string CategoryId { get; set; } = default!;
        public List<UnitRelationModel> UnitRelations { get; set; } = [];
    }
}
