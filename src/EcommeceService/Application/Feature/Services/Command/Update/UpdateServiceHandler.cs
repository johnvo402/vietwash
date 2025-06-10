using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Services;
using Contracts.Utils;

namespace Application.Feature.Services.Command.Update;

public class UpdateServiceHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IMediaUpdateService<Service> mediaUpdateService
) : IRequestHandler<UpdateServiceCommand, UpdateServiceResponse>
{
    public async ValueTask<UpdateServiceResponse> Handle(UpdateServiceCommand command, CancellationToken cancellationToken)
    {
        Service? existingService = await unitOfWork.Repository<Service>().FindByConditionAsync(new GetServiceWithIncludeByIdSpecification(command.ServiceId), cancellationToken)
            ?? throw new NotFoundException(
     [Messager.Create<Service>().Message(MessageType.Found).Negative().BuildMessage()]
 );

        string? oldServiceImage = existingService.Image;

        mapper.Map(command.Service, existingService);

        existingService.Slug = Generator.GenerateSlug(existingService.Name);

        string? newServiceImage = command.Service.Image;
        try
        {
            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            await unitOfWork.Repository<Service>().UpdateAsync(existingService);

            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            if (!string.IsNullOrEmpty(oldServiceImage))
            {
                await mediaUpdateService.DeleteAvatarAsync(oldServiceImage);
            }

            return mapper.Map<UpdateServiceResponse>(existingService);
        }

        catch
        {
            if (!string.IsNullOrEmpty(newServiceImage))
            {
                await mediaUpdateService.DeleteAvatarAsync(newServiceImage);
            }
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

}


