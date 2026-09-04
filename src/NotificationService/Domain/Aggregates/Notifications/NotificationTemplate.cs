using Shared.Kernel.Common;

namespace Domain.Aggregates.Notifications
{
    public class NotificationTemplate : BaseEntity<string>
    {
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public string ContentHtml { get; set; } = default!;
    }
}
