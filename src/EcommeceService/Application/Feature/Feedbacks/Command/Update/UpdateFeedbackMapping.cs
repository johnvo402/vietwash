using Application.Feature.Common.Projections.Feedbacks;
using Domain.Aggregates.Feedbacks;

namespace Application.Feature.Feedbacks.Command.Update;

public static class UpdateFeedbackMapping
{
    public static void FromUpdateModel(this Feedback entity, UpdateFeedbackModel model)
    {
        entity.Update(comment: model.Comment, rating: model.Rating);
    }
}
