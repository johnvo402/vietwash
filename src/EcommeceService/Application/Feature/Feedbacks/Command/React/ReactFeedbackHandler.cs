using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Feedbacks.Specifications;
using Mediator;

namespace Application.Feature.Feedbacks.Command.React
{
    public class ReactFeedbackHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
        : IRequestHandler<ReactFeedbackCommand, Result>
    {
        public async ValueTask<Result> Handle(
            ReactFeedbackCommand request,
            CancellationToken cancellationToken
        )
        {
            var customerId = (long)currentAccount.Id!;
            Feedback? existingFeedback = await unitOfWork
                .DynamicReadOnlyRepository<Feedback>()
                .FindByConditionAsync(
                    new GetFeedbackWithIncludeByIdSpecification(request.FeedbackId),
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

            var react = existingFeedback.Reactions.FirstOrDefault(r => r.CustomerId == customerId);
            if (react is not null)
            {
                react.Type = request.FeedbackReaction.ReactionType;
            }
            else
            {
                existingFeedback.Reactions.Add(
                    new FeedbackReaction
                    {
                        CustomerId = customerId,
                        Type = request.FeedbackReaction.ReactionType,
                    }
                );
            }
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
