using Domain.Aggregates.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Tariffs.Specifications
{
    public class ListTariffByBranchSpecification : Specification<Tariff>
    {
        public ListTariffByBranchSpecification(long branchId)
        {
            var currentTime = DateTime.UtcNow;
            Query
                .Where(tariff =>
                    tariff.BranchId == branchId
                    && tariff.Status == ActivationStatus.Active
                    && (tariff.StartAt == null || tariff.StartAt <= currentTime)
                    && (tariff.EndAt == null || tariff.EndAt >= currentTime)
                )
                .AsNoTracking()
                .AsSplitQuery();
        }
    }
}
