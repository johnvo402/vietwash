using Contracts.Application.Common.Interfaces.Services.PubSub;
using ProjectService_gRPC;

namespace Contracts.Infrastructure.PubSub
{
    public class PubSubLogServiceClient : IPubSubLogService
    {
        private readonly PubSubLogService.PubSubLogServiceClient _client;

        public PubSubLogServiceClient(PubSubLogService.PubSubLogServiceClient channel)
        {
            _client = channel;
        }

        public async Task<bool> CreateLogAsync(
            CreatePubSubLogRequest request,
            CancellationToken cancellationToken
        )
        {
            var response = await _client.CreatePubSubLogAsync(
                request,
                cancellationToken: cancellationToken
            );
            return response.Success;
        }
    }
}
