using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using Mediator;

namespace Application.Feature.Services.Queries.GetByTariff
{
    public class GetByTariffHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetByTariffQuery, Result<PaginationResponse<GetByTariffResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<GetByTariffResponse>>> Handle(
            GetByTariffQuery request,
            CancellationToken cancellationToken
        )
        {
            var validation = request.Validate<GetByTariffQuery, GetByTariffResponse>();

            if (validation != null)
            {
                return validation;
            }
            var now = DateTimeOffset.UtcNow;
            var response = await unitOfWork
                .DynamicReadOnlyRepository<Category>()
                .PagedListAsync(
                    new ListServiceCategoryByTariffSpecification(request.TariffId, now),
                    request,
                    GetByTariffMapping.Selector(),
                    cancellationToken: cancellationToken
                );
            return Result<PaginationResponse<GetByTariffResponse>>.Success(response);
        }
    }
}
