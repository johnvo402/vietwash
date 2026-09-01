using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Enums;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Funds;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Application.Features.Funds.Events;

public class UpdateStatusOrderEventHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateStatusOrderEvent, PubSubResponse<UpdateStatusOrderEvent>>
{
    public const string SourceEventIndexName = "ix_fund_source_event_id";

    public async ValueTask<PubSubResponse<UpdateStatusOrderEvent>> Handle(
        UpdateStatusOrderEvent request,
        CancellationToken cancellationToken
    )
    {
        if (request.PayloadId == Guid.Empty)
            return Failure(request, "PayloadId is required.", PubSubErrorType.Persistent);

        try
        {
            if (
                await unitOfWork
                    .Repository<Fund>()
                    .AnyAsync(x => x.SourceEventId == request.PayloadId, cancellationToken)
            )
                return Success(request);
        }
        catch (Exception ex)
        {
            return Failure(request, ex.Message, PubSubErrorType.Transient);
        }

        var validationError = FinanceEventValidation.Validate(request);
        if (validationError is not null)
            return Failure(request, validationError, PubSubErrorType.Persistent);

        var payload = request.Payload!;
        var fund = payload.ToFund(request.PayloadId);
        List<Transaction> transactions = [];

        if (payload.FundEventType == FundEventType.Order)
        {
            var earnedPointTransaction = payload.ToTransaction(request.PayloadId);
            if (earnedPointTransaction is not null)
                transactions.Add(earnedPointTransaction);

            if (payload.Point > 0)
                transactions.Add(payload.ToTransactionUsePoint(request.PayloadId));
        }

        var transactionStarted = false;
        try
        {
            _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
            transactionStarted = true;

            await unitOfWork.Repository<Fund>().AddAsync(fund, cancellationToken);
            if (transactions.Count > 0)
            {
                await unitOfWork
                    .Repository<Transaction>()
                    .AddRangeAsync(transactions, cancellationToken);
            }

            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return Success(request);
        }
        catch (DbUpdateException ex) when (IsDuplicateSourceEvent(ex))
        {
            if (transactionStarted)
                await TryRollbackAsync(cancellationToken);

            return Success(request);
        }
        catch (Exception ex)
        {
            if (transactionStarted)
                await TryRollbackAsync(cancellationToken);

            return Failure(request, ex.Message, PubSubErrorType.Transient);
        }
    }

    public static bool IsDuplicateSourceEvent(DbUpdateException exception) =>
        exception.InnerException
            is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: SourceEventIndexName,
            };

    private async Task TryRollbackAsync(CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.RollbackAsync(cancellationToken);
        }
        catch
        {
            // Preserve the original database result. The unit of work is scoped to this delivery.
        }
    }

    private static PubSubResponse<UpdateStatusOrderEvent> Success(UpdateStatusOrderEvent request) =>
        new()
        {
            IsSuccess = true,
            ResponseData = request,
            LastAttemptTime = DateTimeOffset.UtcNow,
            PayloadId = request.PayloadId,
        };

    private static PubSubResponse<UpdateStatusOrderEvent> Failure(
        UpdateStatusOrderEvent request,
        object error,
        PubSubErrorType errorType
    ) =>
        new()
        {
            Error = error,
            ErrorType = errorType,
            IsSuccess = false,
            ResponseData = request,
            LastAttemptTime = DateTimeOffset.UtcNow,
            PayloadId = request.PayloadId,
        };
}
