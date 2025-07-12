using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Feedbacks.Specifications
{
	public class FeedbackReactionByCustomerSpec : Specification<FeedbackReaction>
	{
		public FeedbackReactionByCustomerSpec(long? customerId)
		{
			Query.Where(r => r.CustomerId == customerId)
				.AsNoTracking();
		}
	}
}
