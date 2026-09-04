using Domain.Aggregates.Feedbacks;

namespace Application.Feature.Feedbacks.Command.Reply
{
    public static class CreateReyplyFeedbackMapping
    {
        public static Feedback ToEntityReply(
            this Feedback parentFeedback,
            CreateReyplyFeedbackCommand model,
            long userId
        )
        {
            return new Feedback(
                branchId: parentFeedback.BranchId,
                serviceId: parentFeedback.ServiceId,
                comment: Application.Common.Security.RichTextSanitizer.Sanitize(
                    model.ReplyFeedback.Comment
                ),
                parentId: model.Id,
                userId: userId
            );
        }
    }
}
