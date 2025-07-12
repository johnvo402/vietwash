using System.Data.Common;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Feedbacks;
using Domain.Aggregates.Services;
using Mediator;

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
            var branchId = unitOfWork
                .Repository<Service>()
                .QueryAsync()
                .Where(x => x.Id == request.Id)
                .Select(x => x.BranchId)
                .FirstOrDefault();
            Feedback mappingFeedback = request.ToEntityCreate((long)currentUser.Id!, branchId);
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
