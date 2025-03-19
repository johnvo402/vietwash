using Application.Common.Exceptions;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Services;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Feature.Services.Command.Delete;

public class DeleteServiceHandler(
    IUnitOfWork unitOfWork,
    IMediaUpdateService<Service> MediaUpdateService
) : IRequestHandler<DeleteServiceCommand>
{
    public async ValueTask<Mediator.Unit> Handle(
        DeleteServiceCommand command,
        CancellationToken cancellationToken
    )
    {
        Service getService =
            await unitOfWork.Repository<Service>().FindByIdAsync(command.ServiceId)
            ?? throw new NotFoundException(
                [Messager.Create<Service>().Message(MessageType.Found).Negative().BuildMessage()]
            );

        string? oldServiceImage = getService.Image;

        await unitOfWork.Repository<Service>().DeleteAsync(getService);
        await unitOfWork.SaveAsync(cancellationToken);

        await MediaUpdateService.DeleteAvatarAsync(oldServiceImage);
        return Mediator.Unit.Value;
    }
}
