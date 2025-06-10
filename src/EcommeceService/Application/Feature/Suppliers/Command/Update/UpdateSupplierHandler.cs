using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Suppliers;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using System.Data.Common;

namespace Application.Feature.Suppliers.Command.Update
{
    public class UpdateSupplierHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper
    )
        : IRequestHandler<UpdateSupplierCommand, UpdateSupplierResponse>
    {
        public async ValueTask<UpdateSupplierResponse> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
        {
            Supplier? existingSupplier = await unitOfWork.Repository<Supplier>().FindByConditionAsync(s => s.Id == request.SupplierId && !s.Disable, cancellationToken)

                        ?? throw new NotFoundException(
                 [Messager.Create<Supplier>().Message(MessageType.Found).Negative().BuildMessage()]
             );
            if (request.Body.Status.HasValue)
            {
                existingSupplier.Status = request.Body.Status.Value;
            }
            mapper.Map(request.Body.Supplier, existingSupplier);
            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

                await unitOfWork.Repository<Supplier>().UpdateAsync(existingSupplier);
                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                return mapper.Map<UpdateSupplierResponse>(existingSupplier);
            }
            catch
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
