using Domain.Aggregates.Tariffs;

namespace Application.Feature.Tariffs.Queries.Detail
{
	public static class GetTariffDetailMapping
	{
		public static GetTariffDetailResponse ToGetTariffDetailResponse(this Tariff tariff)
		{
			GetTariffDetailResponse response = new GetTariffDetailResponse();
			response.MappingFrom(tariff);
			return response;
		}
	}
}
