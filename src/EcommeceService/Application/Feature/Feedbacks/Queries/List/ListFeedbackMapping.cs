using Application.Feature.Common.Projections.Feedbacks;
using Application.Feature.Services.Queries.Detail;
using Domain.Aggregates.Feedbacks;
using System.Linq.Expressions;

namespace Application.Feature.Feedbacks.Queries.List
{
	public class ListFeedbackMapping
	{
		public static Expression<Func<Feedback, ListFeedbackResponse>> Selector()
		{
			return feedback => new ListFeedbackResponse
			{
				Id = feedback.Id,
				BranchId = feedback.BranchId,
				ServiceId = feedback.ServiceId,
				CustomerId = feedback.CustomerId,

				Comment = feedback.Comment,
				Rating = feedback.Rating,
				Likes = feedback.Likes,
				Dislikes = feedback.Dislikes,

				CreatedUser = feedback.Customer != null
					? new UserDTO
					{
						Id = feedback.Customer.Id,
						DisplayName = feedback.Customer.DisplayName,
						Email = feedback.Customer.Email,
						Avatar = feedback.Customer.AvtUrl
					}
					: null,
				Replies = feedback.Replies.Select(reply => new ReplyProjection
				{
					Id = reply.Id,
					StaffId = reply.StaffId,
					Comment = reply.Comment,
					CreatedAt = reply.CreatedAt,
					CreatedUser = reply.Staff != null
						? new UserDTO
						{
							Id = reply.Staff.Id,
							DisplayName = reply.Staff.DisplayName,
							Email = reply.Staff.Email,
							Avatar = reply.Staff.AvtUrl
						}
						: null
				}).ToList()
			};
		}
	}
}
