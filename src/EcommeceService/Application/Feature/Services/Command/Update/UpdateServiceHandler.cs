using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Contracts.Utils;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using Mediator;

namespace Application.Feature.Services.Command.Update;

public class UpdateServiceHandler(IUnitOfWork unitOfWork, IMediaUpdateService mediaUpdateService)
    : IRequestHandler<UpdateServiceCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateServiceCommand command,
        CancellationToken cancellationToken
    )
    {
        Service? existingService = await unitOfWork
            .DynamicReadOnlyRepository<Service>()
            .FindByConditionAsync(
                new GetServiceWithIncludeByIdSpecification(command.ServiceId),
                cancellationToken
            );
        if (existingService == null)
        {
            return Result.Failure(
                new NotFoundError(
                    "Service not found",
                    Messager.Create<Service>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }

        string? oldServiceImage = existingService.Image;

        existingService.FromUpdateModel(command.Service);

        existingService.Slug = Generator.GenerateSlug(existingService.Name);

        string? newServiceImage = command.Service.Image;
        try
        {
            DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            await unitOfWork.Repository<Service>().UpdateAsync(existingService);

            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            if (!string.IsNullOrEmpty(oldServiceImage))
            {
                await mediaUpdateService.DeleteAvatarAsync(oldServiceImage);
            }

            return Result.Success();
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
