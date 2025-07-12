using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Feedbacks;
using Mediator;
using System.Data.Common;

namespace Application.Feature.Feedbacks.Command.Create
{
	public class CreateFeedbackHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
		: IRequestHandler<CreateFeedbackCommand, Result>
	{
		public async ValueTask<Result> Handle(
			CreateFeedbackCommand request,
			CancellationToken cancellationToken
		)
		{
			Feedback mappingFeedback = request.ToEntityCreate();
			mappingFeedback.CustomerId = currentUser.Session!.Id!;
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
