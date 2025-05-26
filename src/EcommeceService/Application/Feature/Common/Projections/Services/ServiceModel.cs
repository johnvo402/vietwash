using System.Text.Json.Serialization;
using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Services.Enums;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceModel
    {
		public long BranchId { get; set; } = default!;
        public string Name { get; set; } = default!;
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TypeStatus Type { get; set; } = default!;
		public string? Description { get; set; }
        public string? Image { get; set; }
        public long CategoryId { get; set; } = default!;
		public List<UnitRelationModel> UnitRelations { get; set; } = [];
    }
}
