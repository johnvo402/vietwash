using Ardalis.GuardClauses;
using Shared.Kernel.Common;

namespace Domain.Aggregates.PubSubLogs;

public class PubSubLog : BaseEntity
{
    public Guid RequestId { get; set; }
    public object? Request { get; set; }
    public object? ErrorDetail { get; set; }
    public Type ProcessedBy { get; set; } = Type.Origin;
    public int RetryCount { get; set; }

    public PubSubLog(
        Guid requestId,
        object? request,
        object? errorDetail,
        Type processedBy,
        int retryCount
    )
    {
        RequestId = Guard.Against.Default(requestId, nameof(requestId));
        ProcessedBy = Guard.Against.EnumOutOfRange(processedBy, nameof(processedBy));
        Guard.Against.Negative(retryCount, nameof(retryCount));

        Request = request;
        ErrorDetail = errorDetail;
        RetryCount = retryCount;
    }
}
