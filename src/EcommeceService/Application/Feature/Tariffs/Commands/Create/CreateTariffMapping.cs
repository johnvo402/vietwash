using Application.Feature.Common.Projections.Tariffs;
using Domain.Aggregates.Tariffs;

namespace Application.Feature.Tariffs.Commands.Create;

public static class CreateTariffMapping
{
    public static Tariff ToEntityCreate(this TariffModel model)
    {
        return new Tariff(
            name: model.Name.Trim(),
            branchId: model.BranchId,
            disable: model.Disable
        );
    }
}
