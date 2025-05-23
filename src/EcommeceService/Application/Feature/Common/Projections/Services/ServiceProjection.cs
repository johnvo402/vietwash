using System.Text.Json.Serialization;
using Application.Common.Security;
using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Services.Enums;
using JohnChum.SharedKernel.Domain.Common;

namespace Application.Feature.Common.Projections.Services
{
    public class ServiceProjection : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
		public TypeStatus Type { get; set; } = default!;
		public string? Slug { get; set; }

		[File]
        public string? Image { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceStatus? Status { get; set; }
        public long CategoryId { get; set; } = default!;

        public List<UnitRelationProjection> UnitRelations { get; set; } = new();
    }
}
