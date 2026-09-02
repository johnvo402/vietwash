namespace Domain.Aggregates.Orders;

public sealed record OrderCancellation
{
    public const int MinimumReasonLength = 3;
    public const int MaximumReasonLength = 500;

    public DateTimeOffset CancelledAt { get; }
    public long CancelledBy { get; }
    public string Reason { get; }

    private OrderCancellation(DateTimeOffset cancelledAt, long cancelledBy, string reason)
    {
        CancelledAt = cancelledAt.ToUniversalTime();
        CancelledBy = cancelledBy;
        Reason = reason;
    }

    public static OrderCancellation Create(
        DateTimeOffset cancelledAt,
        long cancelledBy,
        string cancellationReason
    )
    {
        if (cancelledBy <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(cancelledBy),
                "The cancelling account id must be positive."
            );

        string reason = cancellationReason?.Trim() ?? string.Empty;
        if (reason.Length is < MinimumReasonLength or > MaximumReasonLength)
            throw new ArgumentException(
                $"Cancellation reason must contain between {MinimumReasonLength} and {MaximumReasonLength} characters.",
                nameof(cancellationReason)
            );

        return new OrderCancellation(cancelledAt, cancelledBy, reason);
    }
}
