using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Feedbacks.Specifications
{
    public class GetFeedbackWithoutIncludeByIdSpecification : Specification<Feedback>
    {
        public GetFeedbackWithoutIncludeByIdSpecification(long id)
        {
            Query.Where(x => x.Id == id && !x.Disable);
        }
    }
}
