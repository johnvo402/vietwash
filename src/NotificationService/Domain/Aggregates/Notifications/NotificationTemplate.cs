using Shared.Kernel.Common;

namespace Domain.Aggregates.Notifications
{
    public class NotificationTemplate : BaseEntity<string>
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ContentHtml { get; set; }
    }
}
