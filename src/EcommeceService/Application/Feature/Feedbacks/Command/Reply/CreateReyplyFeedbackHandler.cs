using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Feedbacks.Specifications;
using Mediator;

namespace Application.Feature.Feedbacks.Command.Reply
{
    public class CreateReyplyFeedbackHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
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
                    new GetFeedbackWithoutIncludeByIdSpecification(request.Id),
                    cancellationToken
                );
            if (parentFeedback == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "The parent feedback is not found",
                        Messager.Create<Feedback>().Message(MessageType.Found).Negative().Build()
                    )
                );
            }

            Feedback mappingFeedback = parentFeedback.ToEntityReply(
                request,
                (long)currentAccount.Id!
            );

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
