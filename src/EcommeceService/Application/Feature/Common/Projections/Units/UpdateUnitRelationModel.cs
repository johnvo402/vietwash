using Domain.Aggregates.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Units
{
	public class UpdateUnitRelationModel
	{
		public long Id { get; set; } = default!;
		public long BranchId { get; set; } = default!;
		public ActivationStatus Status { get; set; } = default!;// khác với unitRelationModel
		public string Name { get; set; } = default!;
		public bool BaseUnit { get; set; } = default!;
		public decimal Price { get; set; } = default!;
		public int Multiple { get; set; } = 1; // Mặc định là 1 cho Service
		public decimal ProcessingTime { get; set; } = default!;
	}
}
