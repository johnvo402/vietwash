using Contracts.Application.Common.Interfaces.Services.Queue;
using Grpc.Net.Client;
using ProjectService_gRPC;


namespace Contracts.Infrastructure.Queue
{
    public class QueueLogServiceClient : IQueueLogService
    {
        private readonly QueueLogService.QueueLogServiceClient _client;

        public QueueLogServiceClient(QueueLogService.QueueLogServiceClient channel)
        {
            _client = channel;
        }
        public async Task<bool> CreateLogAsync(CreateQueueLogRequest request, CancellationToken cancellationToken)
        {
            var response = await _client.CreateQueueLogAsync(request, cancellationToken: cancellationToken);
            return response.Success;
        }
    }
}
