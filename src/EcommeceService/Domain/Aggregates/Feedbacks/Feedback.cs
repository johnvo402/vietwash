using Ardalis.GuardClauses;
using Domain.Aggregates.Services;
using Domain.Aggregates.Users;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Feedbacks
{
    public class Feedback : BaseEntity<long>
    {
        public long BranchId { get; set; }
        public long UserId { get; set; }
        public long ServiceId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public long? ParentId { get; set; } // null nếu là feedback gốc
        public Feedback? Parent { get; set; }
        public ICollection<Feedback> Replies { get; set; } = [];
        public User? User { get; set; }
        public bool Disable { get; set; } = false;
        public ICollection<FeedbackReaction> Reactions { get; set; } = [];
        public Service Service { get; set; }

        public Feedback() { }

        public Feedback(
            long branchId,
            long serviceId,
            string? comment,
            long userId,
            int? rating = null,
            long? parentId = null
        )
        {
            BranchId = Guard.Against.NegativeOrZero(branchId);
            ServiceId = Guard.Against.NegativeOrZero(serviceId);
            Comment = comment;

            // Nếu là Feedback gốc từ Customer
            ParentId = parentId;
            UserId = userId;
            Rating = rating;
        }

        public void Update(string? comment, int? rating = null)
        {
            if (!string.IsNullOrWhiteSpace(comment))
                Comment = comment;
            if (rating.HasValue)
                Rating = rating.Value;
        }
    }
}
