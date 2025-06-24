using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Utils;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;
using Mediator;

namespace Application.Features.Funds.Command.Create
{
    public class CreateFundHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CreateFundCommand, Result>
    {
        public async ValueTask<Result> Handle(
            CreateFundCommand request,
            CancellationToken cancellationToken
        )
        {
            string code = Generator.GenerateCode("FU-", 6);
            Fund fund = request.ToFund(code);

            if (fund.Status == FundStatus.Confirmed)
            {
                fund.TransactionDate = DateTimeOffset.UtcNow;
            }
            else
            {
                fund.TransactionDate = null;
            }

            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                await unitOfWork.Repository<Fund>().AddAsync(fund, cancellationToken);

                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }

            return Result.Success();
        }
    }
}
