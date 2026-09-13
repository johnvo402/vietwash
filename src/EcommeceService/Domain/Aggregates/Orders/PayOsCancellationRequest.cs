namespace Domain.Aggregates.Orders;

public sealed class PayOsCancellationRequest
{
    private PayOsCancellationRequest() { }

    private PayOsCancellationRequest(
        long orderId,
        DateTimeOffset requestedAt,
        long cancelledBy,
        string reason
    )
    {
        OrderId = orderId;
        RequestedAt = requestedAt;
        CancelledBy = cancelledBy;
        Reason = reason;
        State = PayOsCancellationState.Pending;
        NextAttemptAt = requestedAt;
    }

    public long OrderId { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public long CancelledBy { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public PayOsCancellationState State { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }
    public int Attempts { get; private set; }
    public Guid? LeaseId { get; private set; }
    public DateTimeOffset? LockedUntil { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? ProviderState { get; private set; }
    public string? LastError { get; private set; }

    public bool IsTerminalFailure =>
        State == PayOsCancellationState.Failed && NextAttemptAt is null;

    public static PayOsCancellationRequest Create(
        long orderId,
        OrderCancellation cancellation
    ) =>
        new(
            orderId,
            cancellation.CancelledAt,
            cancellation.CancelledBy,
            cancellation.Reason
        );

    public void MarkProcessing(Guid leaseId, DateTimeOffset lockedUntil, int attempts)
    {
        State = PayOsCancellationState.Processing;
        LeaseId = leaseId;
        LockedUntil = lockedUntil;
        Attempts = attempts;
        LastError = null;
    }

    public void MarkCompleted(DateTimeOffset completedAt, string providerState)
    {
        State = PayOsCancellationState.Completed;
        CompletedAt = completedAt;
        ProviderState = providerState;
        NextAttemptAt = null;
        LeaseId = null;
        LockedUntil = null;
        LastError = null;
    }

    public void MarkFailed(
        string providerState,
        string error,
        DateTimeOffset? nextAttemptAt
    )
    {
        State = PayOsCancellationState.Failed;
        ProviderState = providerState;
        LastError = error;
        NextAttemptAt = nextAttemptAt;
        LeaseId = null;
        LockedUntil = null;
    }
}

public enum PayOsCancellationState
{
    Pending,
    Processing,
    Completed,
    Failed,
}
