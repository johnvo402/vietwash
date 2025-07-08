using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Feedbacks.Specifications
{
	public class ListFeedbackSpecification : Specification<Feedback>
	{
		public ListFeedbackSpecification()
		{
			Query
				.Where(x => !x.Disable && x.ParentId == null)
				.Include(x => x.Customer)
				.Include(f => f.Replies).ThenInclude(r => r.Staff)
				.OrderByDescending(x => x.CreatedAt)
				.AsSplitQuery()
				.AsNoTracking();
		}
	}
}
