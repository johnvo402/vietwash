using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Enums;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Funds;
using Mediator;

namespace Application.Features.Funds.Events
{
    public class UpdateStatusOrderEventHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateStatusOrderEvent, PubSubResponse<UpdateStatusOrderEvent>>
    {
        public async ValueTask<PubSubResponse<UpdateStatusOrderEvent>> Handle(
            UpdateStatusOrderEvent request,
            CancellationToken cancellationToken
        )
        {
            Fund fund = request.Payload!.ToFund();
            List<Transaction> transactions = new List<Transaction>();

            if (request.Payload?.FundEventType == FundEventType.Order)
            {
                Transaction transaction = request.Payload!.ToTransaction();
                transactions.Add(transaction);
                if (request.Payload!.Point > 0)
                {
                    Transaction pointTransaction = request.Payload.ToTransactionUsePoint();
                    transactions.Add(pointTransaction);
                }
            }

            try
            {
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

                await unitOfWork.Repository<Fund>().AddAsync(fund, cancellationToken);
                if (transactions.Any())
                {
                    await unitOfWork
                        .Repository<Transaction>()
                        .AddRangeAsync(transactions, cancellationToken);
                }

                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                return new PubSubResponse<UpdateStatusOrderEvent>
                {
                    Error = ex.Message,
                    ErrorType = PubSubErrorType.Transient,
                    IsSuccess = false,
                    ResponseData = request,
                    LastAttemptTime = DateTime.UtcNow,
                    PayloadId = request.PayloadId,
                };
            }
            return new PubSubResponse<UpdateStatusOrderEvent>
            {
                Error = null,
                ErrorType = null,
                IsSuccess = true,
                ResponseData = request,
                LastAttemptTime = DateTime.UtcNow,
                PayloadId = request.PayloadId,
            };
        }
    }
}
