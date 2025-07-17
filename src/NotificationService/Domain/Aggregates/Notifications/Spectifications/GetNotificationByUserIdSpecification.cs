using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Notifications.Spectifications
{
    public class GetNotificationByUserIdSpecification : Specification<Notification>
    {
        public GetNotificationByUserIdSpecification(string userId)
        {
            Query.Where(x => x.UserId == userId).AsSplitQuery().AsNoTracking();
        }
    }
}
