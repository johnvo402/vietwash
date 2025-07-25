using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Tariffs.Specifications;
using Mediator;

namespace Application.Feature.Tariffs.Queries.List
{
    public class ListTariffHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ListTariffQuery, Result<PaginationResponse<ListTariffResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<ListTariffResponse>>> Handle(
            ListTariffQuery request,
            CancellationToken cancellationToken
        )
        {
            var validation = request.Validate<ListTariffQuery, ListTariffResponse>();

            if (validation != null)
            {
                return validation;
            }
            var response = await unitOfWork
                .DynamicReadOnlyRepository<Tariff>()
                .PagedListAsync(
                    new ListTariffSpecification(),
                    request,
                    ListTariffMapping.Selector(),
                    cancellationToken
                );

            return Result<PaginationResponse<ListTariffResponse>>.Success(response);
        }
    }
}
