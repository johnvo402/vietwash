using Domain.Aggregates.Feedbacks.Enums;
using Domain.Aggregates.Users;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Feedbacks
{
    public class FeedbackReaction : DefaultEntity<long>
    {
        public long FeedbackId { get; set; }
        public long CustomerId { get; set; }
        public ReactionType Type { get; set; }
        public Feedback Feedback { get; set; } = default!;
        public User Customer { get; set; } = default!;
    }
}
