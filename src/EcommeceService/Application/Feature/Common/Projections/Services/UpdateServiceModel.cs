using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Services
{
	public class UpdateServiceModel
	{
		public long BranchId { get; set; } = default!;
		public string Name { get; set; } = default!;
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TypeStatus Type { get; set; } = default!;
		public string? Description { get; set; }
		public string? Image { get; set; }
		public long CategoryId { get; set; } = default!;
		public List<UpdateUnitRelationModel> UnitRelations { get; set; } = [];
	}
}
