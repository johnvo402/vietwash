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
                .Include(x => x.User)
                .Include(f => f.Replies)
                .Include(x => x.Reactions)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking();
        }
    }
}
