using Application.Common.Interfaces.Registers;
using Domain.Aggregates.PubSubLogs;

namespace Application.Common.Interfaces.Services.DistributedCache;

public interface IPubSubFactory : ISingleton
{
    IPubSubService GetPubSub(PubSubType type);
}
