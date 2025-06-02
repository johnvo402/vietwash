using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Suppliers;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using System.Data.Common;


namespace Application.Feature.Suppliers.Command.Delete
{
	public class DeleteSupplierHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteSupplierCommand>
	{
		public async ValueTask<Unit> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
		{
			Supplier existingSupplier = await unitOfWork.Repository<Supplier>()
			.FindByConditionAsync(s => s.Id == request.SupplierId, cancellationToken)
			?? throw new NotFoundException(
				[Messager.Create<Supplier>().Message(MessageType.Found).Negative().BuildMessage()]
			);
		
			try
			{
				DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

				await unitOfWork.Repository<Supplier>().DeleteAsync(existingSupplier);
				await unitOfWork.SaveAsync(cancellationToken);
				await unitOfWork.CommitAsync(cancellationToken);
				return Unit.Value;
			}
			catch
			{
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
