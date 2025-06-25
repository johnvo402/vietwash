using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Utils;
using Domain.Aggregates.Suppliers;
using Mediator;

namespace Application.Feature.Suppliers.Command.Create
{
    public class CreateSupplierHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CreateSupplierCommand, Result>
    {
        public async ValueTask<Result> Handle(
            CreateSupplierCommand request,
            CancellationToken cancellationToken
        )
        {
            Supplier supplier = request.ToEntity();
            if (string.IsNullOrEmpty(supplier.Code))
            {
                supplier.Code = Generator.GenerateCode("SUP", 6);
            }

            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                await unitOfWork.Repository<Supplier>().AddAsync(supplier, cancellationToken);
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
}
