using Application.Feature.Common.Projections.Feedbacks;
using Domain.Aggregates.Feedbacks;

namespace Application.Feature.Feedbacks.Command.Create
{
    public static class CreateFeedbackMapping
    {
        public static Feedback ToEntityCreate(
            this CreateFeedbackCommand model,
            long userId,
            long branchId
        )
        {
            return new Feedback(
                branchId: branchId,
                serviceId: model.Id,
                comment: Application.Common.Security.RichTextSanitizer.Sanitize(
                    model.FeedbackModel.Comment
                ),
                rating: model.FeedbackModel.Rating,
                userId: userId
            );
        }
    }
}
