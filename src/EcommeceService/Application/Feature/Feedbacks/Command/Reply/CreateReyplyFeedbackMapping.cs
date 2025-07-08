using Domain.Aggregates.Feedbacks;

namespace Application.Feature.Feedbacks.Command.Reply
{
	public static class CreateReyplyFeedbackMapping
	{
		public static Feedback ToEntityReply(this CreateReyplyFeedbackCommand model, Feedback parentFeedback)
		{
			return new Feedback(
				branchId: parentFeedback.BranchId,
				serviceId: parentFeedback.ServiceId,
				comment: model.Comment,
				parentId: model.ParentId,
				staffId: model.StaffId
			);
		}
	}
}
