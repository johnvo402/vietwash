using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using Mediator;

namespace Application.Feature.Feedbacks.Command.Delete
{
    public class DeleteFeedbackHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteFeedbackCommand, Result>
    {
        public async ValueTask<Result> Handle(
            DeleteFeedbackCommand request,
            CancellationToken cancellationToken
        )
        {
            Feedback? existingFeedback = await unitOfWork
                .Repository<Feedback>()
                .FindByIdAsync(request.FeedbackId);
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

            existingFeedback.Disable = true;

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
