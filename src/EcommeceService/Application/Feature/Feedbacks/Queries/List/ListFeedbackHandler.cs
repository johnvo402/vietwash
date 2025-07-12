using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Mediator;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Feedbacks.Specifications;
using Application.Common.Interfaces.Services;


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
			try
			{
				var validation = query.Validate<ListFeedbackQuery, ListFeedbackResponse>();

				if (validation != null)
				{
					return validation;
				}

				var userReactions = await unitOfWork
					.DynamicReadOnlyRepository<FeedbackReaction>()
					.ListAsync(
						new FeedbackReactionByCustomerSpec(currentCustomer.Id), 
						query, 
						cancellationToken
					);
				var reactionDict = userReactions.ToDictionary(
					r => r.FeedbackId,
					r => (bool?)r.IsLike
				);


				var response = await unitOfWork
					.DynamicReadOnlyRepository<Feedback>()
					.PagedListAsync(
						new ListFeedbackSpecification(),
						query,
						ListFeedbackMapping.Selector(reactionDict),
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