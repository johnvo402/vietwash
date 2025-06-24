using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Funds;
using Mediator;

namespace Application.Features.FundBehaviors.Command
{
    public class CreateFundBehaviorHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CreateFundBehaviorCommand, Result>
    {
        public async ValueTask<Result> Handle(
            CreateFundBehaviorCommand command,
            CancellationToken cancellationToken
        )
        {
            var fundBehavior = command.ToEntity();

            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                await unitOfWork
                    .Repository<FundBehavior>()
                    .AddAsync(fundBehavior, cancellationToken);

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
