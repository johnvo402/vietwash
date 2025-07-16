using Application.Feature.Common.Projections.Tariffs;
using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Services;
using Domain.Aggregates.Tariffs;

namespace Application.Feature.Common.Mapping.Tariffs
{
    public static class TariffMapping 
    {
		public static ServiceTariffProjection ToServiceTariffProjectionResponse(this ServiceTariff st)
		{
			return new()
			{
				TariffId = st.TariffId,
				ServiceId = st.ServiceId,
				UnitRelationId = st.UnitRelationId,
				ServiceName = st.Service.Name,
				Price = st.Price,
				UnitName = st.UnitRelation.Name
			};
		}

		public static List<ServiceTariff>? ToListServiceTariff(
			this List<ServiceTariffModel>? serviceTariffs
		) => serviceTariffs?.Select(ToServiceTariffEntity).ToList();

		public static ServiceTariff ToServiceTariffEntity(this ServiceTariffModel model)
		{
			return new ServiceTariff
			{
				ServiceId = model.ServiceId,
				UnitRelationId = model.UnitRelationId,
				Price = model.Price
			};
		}
	}
}
