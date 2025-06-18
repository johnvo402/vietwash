using Domain.Aggregates.PubSubLogs;
using Mediator;

namespace Application.Features.PubSubLogs;

public class CreatePubSubLogCommand : IRequest
{
    public Guid RequestId { get; set; }
    public object? Request { get; set; }
    public object? ErrorDetail { get; set; }
    public PubSubType ProcessedBy { get; set; } = PubSubType.Origin;
    public int RetryCount { get; set; }
}
