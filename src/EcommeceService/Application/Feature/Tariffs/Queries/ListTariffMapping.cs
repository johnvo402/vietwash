using System.Linq.Expressions;
using Domain.Aggregates.Tariffs;

namespace Application.Feature.Tariffs.Queries.List
{
    public static class ListTariffMapping
    {
        public static Expression<Func<Tariff, ListTariffResponse>> Selector() =>
            tariff => new ListTariffResponse
            {
                Id = tariff.Id,
                PublicId = tariff.PublicId,
                CreatedAt = tariff.CreatedAt,
                CreatedBy = tariff.CreatedBy,
                UpdatedAt = tariff.UpdatedAt,
                UpdatedBy = tariff.UpdatedBy,

                Name = tariff.Name,
                Disable = tariff.Disable,
            };
    }
}
