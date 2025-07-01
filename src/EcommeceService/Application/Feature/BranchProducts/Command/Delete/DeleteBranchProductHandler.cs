using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using Mediator;
using System.Data.Common;


namespace Application.Feature.BranchProducts.Command.Delete
{
	public class DeleteBranchProductHandler(IUnitOfWork unitOfWork)
	: IRequestHandler<DeleteBranchProductCommand, Result>
	{
		public async ValueTask<Result> Handle(
			DeleteBranchProductCommand request, 
			CancellationToken cancellationToken
		)
		{
			BranchProduct? existingBranchProduct = await unitOfWork
			.Repository<BranchProduct>()
			.FindByIdAsync(request.BranchProductId);
			if (existingBranchProduct == null)
			{
				return Result.Failure(
					new NotFoundError(
						"Branch product not found",
						Messager.Create<BranchProduct>().Message(MessageType.Found).Negative().BuildMessage()
					)
				);
			}

			existingBranchProduct.Disable = true;

			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

				await unitOfWork.Repository<BranchProduct>().UpdateAsync(existingBranchProduct);
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
