using Domain.Aggregates.Feedbacks.Enums;

namespace Application.Feature.Common.Projections.Feedbacks;

public class FeedbackReactionModel
{
    public ReactionType ReactionType { get; set; }
}
