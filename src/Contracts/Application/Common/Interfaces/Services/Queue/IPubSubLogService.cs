using ProjectService_gRPC;

namespace Contracts.Application.Common.Interfaces.Services.PubSub
{
    public interface IPubSubLogService
    {
        Task<bool> CreateLogAsync(
            CreatePubSubLogRequest request,
            CancellationToken cancellationToken
        );
    }
}
