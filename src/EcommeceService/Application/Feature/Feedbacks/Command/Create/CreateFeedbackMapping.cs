using Application.Feature.Common.Projections.Feedbacks;
using Domain.Aggregates.Feedbacks;

namespace Application.Feature.Feedbacks.Command.Create
{
	public static class CreateFeedbackMapping
	{
		public static Feedback ToEntityCreate(this FeedbackModel model)
		{
			return new Feedback(
				branchId: model.BranchId,
				serviceId: model.ServiceId,
				comment: model.Comment,
				rating: model.Rating
			);
		}
	}
}
