using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;
using Domain.Aggregates.Funds.Specifications;
using Mediator;

namespace Application.Features.Funds.Command.Update
{
    public class UpdateFundHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateFundCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateFundCommand command,
            CancellationToken cancellationToken
        )
        {
            Fund? fund = await unitOfWork
                .Repository<Fund>()
                .FindByIdAsync(long.Parse(command.FundId), cancellationToken);
            if (fund == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "Fund not found",
                        Messager.Create<Fund>().Message(MessageType.Found).Negative().BuildMessage()
                    )
                );
            }
            command.UpdateFundModel.MapUpdateToEntity(fund);

            if (
                fund.Status == FundStatus.PendingConfirmation
                && command.UpdateFundModel!.Status == FundStatus.Confirmed
            )
            {
                fund.TransactionDate = DateTimeOffset.UtcNow;
            }

            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                await unitOfWork.Repository<Fund>().UpdateAsync(fund);
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
