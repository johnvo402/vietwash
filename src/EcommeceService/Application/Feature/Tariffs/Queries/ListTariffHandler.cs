using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.QueryStringProcessing;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Tariffs.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Tariffs.Queries
{
    public class ListTariffHandler(IUnitOfWork unitOfWork) : IRequestHandler<ListTariffQuery, PaginationResponse<ListTariffResponse>>
    {
        public async ValueTask<PaginationResponse<ListTariffResponse>> Handle(ListTariffQuery request,
            CancellationToken cancellationToken) => await unitOfWork.CachedRepository<Tariff>()
                                                                    .PagedListAsync<ListTariffResponse>(
                                                                        new ListTariffSpecification(),
                                                                        request.ValidateQuery().ValidateFilter(typeof(ListTariffResponse))
                                                                    );

    }
}