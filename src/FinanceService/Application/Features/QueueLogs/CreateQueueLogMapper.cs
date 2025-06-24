using Domain.Aggregates.PubSubLogs;

namespace Application.Features.PubSubLogs;

public static class CreatePubSubLogMapper
{
    public static PubSubLog MapToEntity(this CreatePubSubLogCommand command)
    {
        return new PubSubLog(
            requestId: command.RequestId,
            request: command.Request,
            errorDetail: command.ErrorDetail,
            processedBy: command.ProcessedBy,
            retryCount: command.RetryCount
        );
    }
}
