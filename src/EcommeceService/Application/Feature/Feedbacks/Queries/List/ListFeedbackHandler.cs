using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Mediator;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Feedbacks.Specifications;


namespace Application.Feature.Feedbacks.Queries.List
{
	public class ListFeedbackHandler(IUnitOfWork unitOfWork)
	: IRequestHandler<ListFeedbackQuery, Result<PaginationResponse<ListFeedbackResponse>>>
	{
		public async ValueTask<Result<PaginationResponse<ListFeedbackResponse>>> Handle(
			ListFeedbackQuery query,
			CancellationToken cancellationToken
		)
		{
			try
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
						ListFeedbackMapping.Selector(),
						cancellationToken
					);

				return Result<PaginationResponse<ListFeedbackResponse>>.Success(response);
			}
			catch (Exception ex)
			{
				throw new Exception("Exception", ex);
			}
		}
	}
}