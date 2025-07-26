using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Tariffs.Queries.ListTariffByBranch
{
    public class ListTariffByBranchQuery : IRequest<Result<IList<ListTariffByBranchResponse>>>
    {
        public long BranchId { get; set; }
    };
}
