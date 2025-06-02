using System.Text.Json.Serialization;
using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Services.Enums;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceModel
    {
		public long BranchId { get; set; } = default!;
        public string Name { get; set; } = default!;
		public TypeStatus Type { get; set; } = default!;
        public ServiceStatus Status { get; set; } = default!;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string CategoryId { get; set; } = default!;
		public List<UnitRelationModel> UnitRelations { get; set; } = [];
    }
}
