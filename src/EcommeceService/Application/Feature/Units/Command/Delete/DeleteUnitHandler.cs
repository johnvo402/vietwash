using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Mediator;
using Unit = Domain.Aggregates.Services.Unit;

namespace Application.Feature.Units.Command.Delete
{
    public class DeleteUnitHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteUnitCommand, Result>
    {
        public async ValueTask<Result> Handle(
            DeleteUnitCommand command,
            CancellationToken cancellationToken
        )
        {
            var unit = await unitOfWork
                .Repository<Unit>()
                .FindByIdAsync(command.UnitId, cancellationToken);
            if (unit == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "Unit not found",
                        Messager.Create<Unit>().Message(MessageType.Found).Negative().BuildMessage()
                    )
                );
            }

            await unitOfWork.Repository<Unit>().DeleteAsync(unit);
            await unitOfWork.SaveAsync(cancellationToken);

            return Result.Success();
        }
    }
}
