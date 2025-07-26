using System.Linq.Expressions;
using Domain.Aggregates.Tariffs;

namespace Application.Feature.Tariffs.Queries.ListTariffByBranch
{
    public static class ListTariffByBranchMapping
    {
        public static Expression<Func<Tariff, ListTariffByBranchResponse>> Selected()
        {
            return tariff => new ListTariffByBranchResponse { Id = tariff.Id, Name = tariff.Name };
        }
    }
}
