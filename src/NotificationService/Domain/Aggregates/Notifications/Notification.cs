using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Notifications
{
    public class Notification : AggregateRoot
    {
        public string TemplateId { get; set; }
        public string UserId { get; set; }
        public Dictionary<string, string>? Parameters { get; set; }
        public Dictionary<string, string>? Data { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public string? ContentHtml { get; set; }
        public bool IsRead { get; set; } = false;
        public NotificationTemplate Template { get; set; }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
