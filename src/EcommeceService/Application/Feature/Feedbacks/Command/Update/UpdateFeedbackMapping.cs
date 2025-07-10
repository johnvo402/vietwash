using Application.Feature.Common.Projections.Feedbacks;
using Domain.Aggregates.Feedbacks;

namespace Application.Feature.Feedbacks.Command.Update;

public static class UpdateFeedbackMapping
{
	public static void FromUpdateModel(this Feedback entity, FeedbackModel model)
	{
		entity.Update(
			branchId: model.BranchId,
			serviceId: model.ServiceId,
			comment: model.Comment,
			customerId: model.CustomerId,
			rating: model.Rating
		);
	}
}
