using Domain.Aggregates.Services;
using JohnChum.SharedKernel.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Tariffs
{
	public class ServicePriceTariffHistory : DefaultEntity
	{
		public long ServiceId { get; set; } = default!;
		public long TariffId { get; set; } = default!;
		public long UnitRelationId { get; set; } = default!;
		public decimal Price { get; set; } = default!;

		public Service Service { get; set; } = default!;
		public Tariff Tariff { get; set; } = default!;
	}
}
