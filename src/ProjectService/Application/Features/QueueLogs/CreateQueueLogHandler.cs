using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.PubSubLogs;
using Mediator;
using Serilog;

namespace Application.Features.PubSubLogs;

public class CreatePubSubLogHandler(IUnitOfWork unitOfWork, ILogger logger)
    : IRequestHandler<CreatePubSubLogCommand>
{
    public async ValueTask<Unit> Handle(
        CreatePubSubLogCommand command,
        CancellationToken cancellationToken
    )
    {
        logger.Information("Pushing request {payloadId} to logging queue.", command.RequestId);
        await unitOfWork.Repository<PubSubLog>().AddAsync(command.ToEntity(), cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
        return Unit.Value;
    }
}
