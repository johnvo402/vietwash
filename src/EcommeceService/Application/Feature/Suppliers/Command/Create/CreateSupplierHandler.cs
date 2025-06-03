using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Suppliers;
using Mediator;
using System.Data.Common;

namespace Application.Feature.Suppliers.Command.Create
{
	public class CreateSupplierHandler(
		IUnitOfWork unitOfWork,
		IMapper mapper) : IRequestHandler<CreateSupplierCommand>
	{
		public async ValueTask<Unit> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
		{
			var supplier = mapper.Map<Supplier>(request);
			supplier.Code = $"SUP-{DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()[^6..]}";

			try
			{
				DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

				await unitOfWork.Repository<Supplier>().AddAsync(supplier, cancellationToken);
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
