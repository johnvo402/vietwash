
namespace Application.Feature.Common.Projections.Tariffs
{
	public class ServiceTariffProjection
	{
		public long TariffId { get; set; } = default!;
		public long ServiceId { get; set; }

		public long UnitRelationId = default!;
		public string ServiceName { get; set; } = default!;
		public decimal Price { get; set; }
		public string UnitName { get; set; } = default!;
	}
}
