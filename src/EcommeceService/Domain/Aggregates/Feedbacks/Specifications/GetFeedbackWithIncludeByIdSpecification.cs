using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Feedbacks.Specifications
{
	public class GetFeedbackWithIncludeByIdSpecification : Specification<Feedback>
	{
		public GetFeedbackWithIncludeByIdSpecification(long id)
		{
			Query
				.Where(x => x.Id == id && !x.Disable);
		}
	}
}
