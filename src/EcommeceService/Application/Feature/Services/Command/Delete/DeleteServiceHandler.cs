using System.Data.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Services;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Extensions.Reflections;
using Mediator;

namespace Application.Feature.Services.Command.Delete;

public class DeleteServiceHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteServiceCommand>
{
    public async ValueTask<Mediator.Unit> Handle(
        DeleteServiceCommand command,
        CancellationToken cancellationToken
    )
    {
		if (command.ServiceId < 0)
		{
			throw new ArgumentException("ServiceId must be a positive number", nameof(command.ServiceId));
		}
		Service existingService =
            await unitOfWork.Repository<Service>().FindByIdAsync(command.ServiceId)
            ?? throw new NotFoundException(
                [Messager.Create<Service>().Message(MessageType.Found).Negative().BuildMessage()]
            );

		existingService.Disable = true;

        try
        {
            DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

            await unitOfWork.Repository<Service>().UpdateAsync(existingService);
            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return Mediator.Unit.Value;
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
