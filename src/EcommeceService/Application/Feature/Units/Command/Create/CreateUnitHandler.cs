using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Mediator;
using Unit = Domain.Aggregates.Services.Unit;

namespace Application.Feature.Units.Command.Create
{
    public class CreateUnitHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CreateUnitCommand, Result>
    {
        public async ValueTask<Result> Handle(
            CreateUnitCommand request,
            CancellationToken cancellationToken
        )
        {
            Unit mappingUnit = request.ToUnit();
            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                Unit unit = await unitOfWork
                    .Repository<Unit>()
                    .AddAsync(mappingUnit, cancellationToken);

                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
