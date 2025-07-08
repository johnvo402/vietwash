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
			Feedback? existingFeedback = await unitOfWork
				.Repository<Feedback>()
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

			if (request.IsLike) existingFeedback.Likes++; else existingFeedback.Dislikes++;
			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
					cancellationToken
				);

				await unitOfWork.Repository<Feedback>().UpdateAsync(existingFeedback);
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
