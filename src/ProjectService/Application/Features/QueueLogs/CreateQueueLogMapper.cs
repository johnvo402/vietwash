using Domain.Aggregates.PubSubLogs;

namespace Application.Features.PubSubLogs
{
    public static class PubSubLogMapper
    {
        public static PubSubLog ToEntity(this CreatePubSubLogCommand command)
        {
            return new PubSubLog(
                command.RequestId,
                command.Request,
                command.ErrorDetail,
                command.ProcessedBy,
                command.RetryCount
            );
        }
    }
}
