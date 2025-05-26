using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Funds;
using Mediator;
using System.Data.Common;

namespace Application.Features.FundBehaviors.Command
{
    public class CreateFundBehaviorHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateFundBehaviorCommand>
    {
        public async ValueTask<Unit> Handle(CreateFundBehaviorCommand command, CancellationToken cancellationToken)
        {
            var fundBehavior = mapper.Map<FundBehavior>(command);

            if (fundBehavior == null)
            {
                return Unit.Value;
            }

            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

                await unitOfWork.Repository<FundBehavior>().AddAsync(fundBehavior, cancellationToken);

                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);


                return Unit.Value;
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }

        }
    }
}
