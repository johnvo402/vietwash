using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Feedbacks.Specifications;
using Mediator;

namespace Application.Feature.Feedbacks.Queries.List
{
    public class ListFeedbackHandler(IUnitOfWork unitOfWork, ICurrentAccount currentCustomer)
        : IRequestHandler<ListFeedbackQuery, Result<PaginationResponse<ListFeedbackResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<ListFeedbackResponse>>> Handle(
            ListFeedbackQuery query,
            CancellationToken cancellationToken
        )
        {
            var validation = query.Validate<ListFeedbackQuery, ListFeedbackResponse>();

            if (validation != null)
            {
                return validation;
            }

            var response = await unitOfWork
                .DynamicReadOnlyRepository<Feedback>()
                .PagedListAsync(
                    new ListFeedbackSpecification(),
                    query,
                    ListFeedbackMapping.Selector((long)currentCustomer.Id!),
                    cancellationToken
                );

            return Result<PaginationResponse<ListFeedbackResponse>>.Success(response);
        }
    }
}
