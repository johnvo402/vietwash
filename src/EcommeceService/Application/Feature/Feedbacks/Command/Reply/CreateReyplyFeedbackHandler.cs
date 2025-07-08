using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using Mediator;
using System.Data.Common;
using Domain.Aggregates.Feedbacks.Specifications;

namespace Application.Feature.Feedbacks.Command.Reply
{	
	public class CreateReyplyFeedbackHandler(IUnitOfWork unitOfWork)
		: IRequestHandler<CreateReyplyFeedbackCommand, Result>
	{
		public async ValueTask<Result> Handle(
			CreateReyplyFeedbackCommand request,
			CancellationToken cancellationToken
		)
		{
			var parentFeedback = await unitOfWork
				.DynamicReadOnlyRepository<Feedback>()
				.FindByConditionAsync(
					new GetFeedbackWithIncludeByIdSpecification(request.ParentId),
					cancellationToken
				);
			if (parentFeedback == null)
			{
				return Result.Failure(
					new NotFoundError(
						"The parent feedback is not found",
						Messager
							.Create<Feedback>()
							.Message(MessageType.Found)
							.Negative()
							.Build()
					)
				);
			}

			Feedback mappingFeedback = request.ToEntityReply(parentFeedback);

			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
					cancellationToken
				);

				Feedback feedback = await unitOfWork
					.Repository<Feedback>()
					.AddAsync(mappingFeedback, cancellationToken);

				await unitOfWork.SaveAsync(cancellationToken);
				await unitOfWork.CommitAsync(cancellationToken);

				return Result.Success();
			}
			catch (Exception)
			{
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
