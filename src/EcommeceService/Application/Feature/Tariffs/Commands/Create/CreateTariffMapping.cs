using Application.Feature.Common.Mapping.Tariffs;
using Application.Feature.Common.Projections.Tariffs;
using Domain.Aggregates.Tariffs;

namespace Application.Feature.Tariffs.Commands.Create;

public static class CreateTariffMapping
{
    public static Tariff ToEntityCreate(this TariffModel model)
    {
        var response = new Tariff(
            name: model.Name,
            branchId: model.BranchId,
            status: model.Status,
			startAt: model.StartAt,
			endAt: model.EndAt
		);
		response.ServiceTariffs = model.ServiceTariffs.ToListServiceTariff() ?? [];
		return response;
	}
}
