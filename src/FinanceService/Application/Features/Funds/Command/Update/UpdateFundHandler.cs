using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Features.Funds.Command.Update
{
    public class UpdateFundHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<UpdateFundCommand>
    {
        public async ValueTask<Unit> Handle(
            UpdateFundCommand command,
            CancellationToken cancellationToken
        )
        {
            Fund fund =
                await unitOfWork
                    .Repository<Fund>()
                    .FindByConditionAsync(
                        new GetFundByIdSpecification(long.Parse(command.FundId)),
                        cancellationToken
                    )
                ?? throw new NotFoundException(
                    [Messager.Create<Fund>().Message(MessageType.Found).Negative().BuildMessage()]
                );
            if (
                fund.Status.Equals("PendingConfirmation")
                && command.updateFundModel!.Status.Equals("Confirmed")
            )
            {
                command.updateFundModel.TransactionDate = DateTimeOffset.UtcNow;
            }
            mapper.Map(command.updateFundModel, fund);

            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(
                    cancellationToken
                );

                await unitOfWork.Repository<Fund>().UpdateAsync(fund);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                return Unit.Value;
            }
            catch
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
