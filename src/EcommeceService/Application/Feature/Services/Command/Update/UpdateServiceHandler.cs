using System.Data.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Services;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;

namespace Application.Feature.Services.Command.Update;

public class UpdateServiceHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediaUpdateService<Service> mediaUpdateService
) : IRequestHandler<UpdateServiceCommand, UpdateServiceResponse>
{
    public async ValueTask<UpdateServiceResponse> Handle(
        UpdateServiceCommand command,
        CancellationToken cancellationToken
    )
    {
        Service? getService =
            await unitOfWork.Repository<Service>().FindByIdAsync(Ulid.Parse(command.ServiceId))
                ?? throw new NotFoundException(
                [Messager.Create<Service>().Message(MessageType.Found).Negative().BuildMessage()]
            );

        string? oldServiceImage = getService.Image;
        mapper.Map(command.Service, getService);
        try
        {
            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            await unitOfWork.Repository<Service>().UpdateAsync(getService);

            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.Repository<Service>().UpdateAsync(getService);
            await unitOfWork.CommitAsync(cancellationToken);
            await mediaUpdateService.DeleteAvatarAsync(oldServiceImage);

            return mapper.Map<UpdateServiceResponse>(getService);
        }
        catch
        {
            await mediaUpdateService.DeleteAvatarAsync(getService.Image);
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
