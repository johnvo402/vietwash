using System;
using Domain.Aggregates.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Services.Specifications
{
    public class ListServiceCategoryByTariffSpecification : Specification<Category>
    {
        public ListServiceCategoryByTariffSpecification(long tariffId, DateTimeOffset currentTime)
        {
            Query
                .Include(c => c.Services)
                .ThenInclude(s => s.UnitRelations)
                .Include(c => c.Services)
                .ThenInclude(s => s.ServiceTariffs)
                .ThenInclude(st => st.Tariff)
                .Where(c =>
                    c.Status == ActivationStatus.Active
                    && c.Services.Any(s =>
                        s.Status == ActivationStatus.Active
                        && s.ServiceTariffs.Any(st =>
                            st.TariffId == tariffId
                            && st.Tariff.Status == ActivationStatus.Active
                            && (st.Tariff.StartAt == null || st.Tariff.StartAt <= currentTime)
                            && (st.Tariff.EndAt == null || st.Tariff.EndAt >= currentTime)
                        )
                    )
                )
                .AsNoTracking()
                .AsSplitQuery();
        }
    }
}
