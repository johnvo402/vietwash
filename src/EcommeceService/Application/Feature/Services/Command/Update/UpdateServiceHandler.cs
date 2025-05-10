using System.Data;
using System.Data.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Services.Command.Update;

public class UpdateServiceHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediaUpdateService<Service> mediaUpdateService,
    IServiceLaundryService serviceLaundryService
) : IRequestHandler<UpdateServiceCommand, UpdateServiceResponse>
{
    public async ValueTask<UpdateServiceResponse> Handle(UpdateServiceCommand command, CancellationToken cancellationToken)
    {
        Service? getService = await unitOfWork.Repository<Service>().FindByConditionAsync(new GetServiceWithIncludeByIdSpecification(Ulid.Parse(command.ServiceId)), cancellationToken)
            ?? throw new NotFoundException(
     [Messager.Create<Service>().Message(MessageType.Found).Negative().BuildMessage()]
 );

        string? oldServiceImage = getService.Image;

        mapper.Map(command.Service, getService);

        var unitRelations = mapper.Map<List<UnitRelation>>(command.Service.UnitRelations);

        try
        {
            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            await serviceLaundryService.UpdateServiceAsync(getService, unitRelations, transaction);

            await unitOfWork.CommitAsync(cancellationToken);

            if (!string.IsNullOrEmpty(oldServiceImage))
            {
                await mediaUpdateService.DeleteAvatarAsync(oldServiceImage);
            }

            return mapper.Map<UpdateServiceResponse>(getService);
        }

        catch
        {
            if (!string.IsNullOrEmpty(getService.Image))
            {
                await mediaUpdateService.DeleteAvatarAsync(getService.Image);
            }
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
