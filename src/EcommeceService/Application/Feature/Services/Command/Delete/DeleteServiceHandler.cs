using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Services;
using Mediator;

namespace Application.Feature.Services.Command.Delete;

public class DeleteServiceHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteServiceCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteServiceCommand command,
        CancellationToken cancellationToken
    )
    {
        Service? existingService = await unitOfWork
            .Repository<Service>()
            .FindByIdAsync(command.ServiceId);

        if (existingService == null)
        {
            return Result.Failure(
                new NotFoundError(
                    "Service not found",
                    Messager.Create<Service>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );
        }

        existingService.Disable = true;

        try
        {
            DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            await unitOfWork.Repository<Service>().UpdateAsync(existingService);
            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
