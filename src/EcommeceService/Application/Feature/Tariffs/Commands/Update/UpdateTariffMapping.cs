using Application.Feature.Common.Projections.Tariffs;
using Domain.Aggregates.Tariffs;

namespace Application.Feature.Tariffs.Commands.Update
{
    public static class UpdateTariffMapping
    {
        public static Tariff FromUpdateTariff(this Tariff tariff, TariffModel update)
        {
            tariff.Update(
                name: update.Name, 
                branchId: update.BranchId,
				status: update.Status,
			    startAt: update.StartAt,
			    endAt: update.EndAt
			);
            return tariff;
        }
    }
}
