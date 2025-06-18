
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.PubSubLogs;

public class PubSubLog : BaseEntity
{
    public Guid RequestId { get; set; }
    public object? Request { get; set; }
    public object? ErrorDetail { get; set; }
    public PubSubType ProcessedBy { get; set; } = PubSubType.Origin;
    public int RetryCount { get; set; }
}
