using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;
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
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

                await unitOfWork.Repository<Fund>().AddAsync(fund, cancellationToken);

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
