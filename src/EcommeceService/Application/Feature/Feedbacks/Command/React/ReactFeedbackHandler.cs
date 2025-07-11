using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Suppliers.Command.Update;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Suppliers;
using Mediator;
using System.Data.Common;

namespace Application.Feature.Feedbacks.Command.React
{
	public class ReactFeedbackHandler(IUnitOfWork unitOfWork)
		: IRequestHandler<ReactFeedbackCommand, Result>
	{
		public async ValueTask<Result> Handle(
			ReactFeedbackCommand request,
			CancellationToken cancellationToken
		)
		{
			var feedbackRepo = unitOfWork.Repository<Feedback>();
			var reactionRepo = unitOfWork.Repository<FeedbackReaction>();

			Feedback? existingFeedback = await feedbackRepo
				.FindByConditionAsync(
					s => s.Id == request.FeedbackId && !s.Disable,
					cancellationToken
				);
			if (existingFeedback == null)
			{
				return Result.Failure(
					new NotFoundError(
						"Feedback not found",
						Messager
							.Create<Feedback>()
							.Message(MessageType.Found)
							.Negative()
							.BuildMessage()
					)
				);
			}

			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
					cancellationToken
				);

				var reaction = await unitOfWork
				.Repository<FeedbackReaction>()
				.FindByConditionAsync(r =>
					r.FeedbackId == request.FeedbackId &&
					r.CustomerId == request.FeedbackReaction.CustomerId,
					cancellationToken
				);
				if (reaction == null)
				{
					await reactionRepo.AddAsync(new FeedbackReaction
					{
						FeedbackId = request.FeedbackId,
						CustomerId = request.FeedbackReaction.CustomerId,
						IsLike = request.FeedbackReaction.IsLike
					});

					if (request.FeedbackReaction.IsLike) existingFeedback.Likes++;
					else existingFeedback.Dislikes++;
				}
				else if (reaction.IsLike != request.FeedbackReaction.IsLike)
				{
					if (request.FeedbackReaction.IsLike)
					{
						existingFeedback.Likes++;
						existingFeedback.Dislikes--;
					}
					else
					{
						existingFeedback.Dislikes++;
						existingFeedback.Likes--;
					}

					reaction.IsLike = request.FeedbackReaction.IsLike;
					await reactionRepo.UpdateAsync(reaction);
				}
				else
				{
					return Result.Success();
				} 
					

				await feedbackRepo.UpdateAsync(existingFeedback);
				await unitOfWork.SaveAsync(cancellationToken);
				await unitOfWork.CommitAsync(cancellationToken);

				return Result.Success();
			}
			catch
			{
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
