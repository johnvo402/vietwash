using System.Linq.Expressions;
using Application.Feature.Common.Projections.Feedbacks;
using Application.Features.Common.Mapping.Users;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Feedbacks.Enums;

namespace Application.Feature.Feedbacks.Queries.List
{
    public class ListFeedbackMapping
    {
        public static Expression<Func<Feedback, ListFeedbackResponse>> Selector(long customerId)
        {
            return feedback => new ListFeedbackResponse
            {
                Id = feedback.Id,
                BranchId = feedback.BranchId,
                ServiceId = feedback.ServiceId,
                CustomerId = feedback.UserId,

                Comment = feedback.Comment,
                Rating = feedback.Rating,
                Likes = feedback.Reactions.Where(r => r.Type == ReactionType.Liked).Count(),
                Dislikes = feedback.Reactions.Where(r => r.Type == ReactionType.Disliked).Count(),
                ReactionType = feedback
                    .Reactions.Where(r => r.CustomerId == customerId)
                    .Select(r => r.Type)
                    .FirstOrDefault(),
                CreatedUser = feedback.User != null ? feedback.User.UserDTOResponse() : null,
                CreatedAt = feedback.CreatedAt,
                UpdatedAt = feedback.UpdatedAt,
                Replies = feedback
                    .Replies.Select(reply => new ReplyProjection
                    {
                        Id = reply.Id,
                        StaffId = reply.UserId,
                        Comment = reply.Comment!,
                        CreatedAt = reply.CreatedAt,
                        CreatedUser = reply.User != null ? reply.User.UserDTOResponse() : null,
                    })
                    .OrderByDescending(x => x.CreatedAt)
                    .ToList(),
            };
        }
    }
}
