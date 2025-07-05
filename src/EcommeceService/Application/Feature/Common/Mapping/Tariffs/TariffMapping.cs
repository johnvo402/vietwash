using Application.Feature.Common.Projections.Tariffs;
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
	}
}
