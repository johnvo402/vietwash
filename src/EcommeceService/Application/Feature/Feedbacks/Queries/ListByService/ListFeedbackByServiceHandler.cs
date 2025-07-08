using Application.Common.Interfaces.UnitOfWorks;
using Mediator;
using Contracts.ApiWrapper;
using Domain.Aggregates.Feedbacks;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Feedbacks.Queries.ListByService
{
	public class ListFeedbackByServiceHandler(IUnitOfWork unitOfWork)
		: IRequestHandler<ListFeedbackByServiceQuery, Result<IEnumerable<ListFeedbackByServiceResponse>>>
	{
		public async ValueTask<Result<IEnumerable<ListFeedbackByServiceResponse>>> Handle(
			ListFeedbackByServiceQuery request,
			CancellationToken cancellationToken
		)
		{
			var feedbacks = await unitOfWork
				.Repository<Feedback>()
				.QueryAsync()
				.Include(x => x.Customer)
				.Include(x => x.Replies)
				.ThenInclude(x => x.Staff)
				.Where(x => x.ServiceId == request.ServiceId && x.ParentId == null && !x.Disable)
				.OrderByDescending(x => x.CreatedAt)
				.Select(ListFeedbackByServiceMapping.Selector())
				.Take(10)
				.AsSplitQuery()
				.ToListAsync(cancellationToken);

			return Result<IEnumerable<ListFeedbackByServiceResponse>>.Success(feedbacks);
		}
	}
}
