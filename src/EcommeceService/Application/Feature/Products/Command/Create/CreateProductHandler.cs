using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Products;
using Mediator;
using System.Data.Common;

namespace Application.Feature.Products.Command.Create
{
	public class CreateProductHandler(IUnitOfWork unitOfWork)
		: IRequestHandler<CreateProductCommand, Result>
	{
		public async ValueTask<Result> Handle(
			CreateProductCommand request, 
			CancellationToken cancellationToken
		)
		{
			Product product = request.ToEntity();
			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
					cancellationToken
				);

				await unitOfWork.Repository<Product>().AddAsync(product, cancellationToken);
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
