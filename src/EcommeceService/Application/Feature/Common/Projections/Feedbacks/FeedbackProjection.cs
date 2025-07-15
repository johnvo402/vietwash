using Application.Features.Common.Projections.Users;
using Domain.Aggregates.Feedbacks.Enums;

namespace Application.Feature.Common.Projections.Feedbacks
{
    public class FeedbackProjection
    {
        public long Id { get; set; }
        public long BranchId { get; set; }
        public long ServiceId { get; set; }
        public long? CustomerId { get; set; }
        public string? Comment { get; set; }
        public int? Rating { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public UserDTO? CreatedUser { get; set; }
        public List<ReplyProjection>? Replies { get; set; }
        public ReactionType ReactionType { get; set; }
    }

    public class ReplyProjection
    {
        public long Id { get; set; }
        public long? StaffId { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public UserDTO? CreatedUser { get; set; }
    }
}
