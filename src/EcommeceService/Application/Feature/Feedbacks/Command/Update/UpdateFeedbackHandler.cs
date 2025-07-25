using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Feedbacks.Specifications;
using Mediator;

namespace Application.Feature.Feedbacks.Command.Update
{
    public class UpdateFeedbackHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateFeedbackCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateFeedbackCommand command,
            CancellationToken cancellationToken
        )
        {
            Feedback? existingFeedback = await unitOfWork
                .DynamicReadOnlyRepository<Feedback>()
                .FindByConditionAsync(
                    new GetFeedbackWithIncludeByIdSpecification(command.FeedbackId),
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
            existingFeedback.FromUpdateModel(command.Feedback);

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
