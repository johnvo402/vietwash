

using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.PubSubLogs;
using Mediator;
using Serilog;

namespace Application.Features.PubSubLogs;

public class CreatePubSubLogHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger logger)
    : IRequestHandler<CreatePubSubLogCommand>
{
    public async ValueTask<Unit> Handle(
        CreatePubSubLogCommand command,
        CancellationToken cancellationToken
    )
    {
        logger.Information("Pushing request {payloadId} to logging queue.", command.RequestId);
        await unitOfWork
            .Repository<PubSubLog>()
            .AddAsync(mapper.Map<PubSubLog>(command), cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        return Unit.Value;
    }
}
