using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Tariffs.Specifications;
using Mediator;

namespace Application.Feature.Tariffs.Queries.ListTariffByBranch
{
    public class ListTariffByBranchByBranchHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ListTariffByBranchQuery, Result<IList<ListTariffByBranchResponse>>>
    {
        public async ValueTask<Result<IList<ListTariffByBranchResponse>>> Handle(
            ListTariffByBranchQuery request,
            CancellationToken cancellationToken
        )
        {
            var response = await unitOfWork
                .DynamicReadOnlyRepository<Tariff>()
                .ListAsync(
                    new ListTariffByBranchSpecification(request.BranchId),
                    new QueryParamRequest(),
                    ListTariffByBranchMapping.Selected(),
                    cancellationToken
                );

            return Result<IList<ListTariffByBranchResponse>>.Success(response);
        }
    }
}
