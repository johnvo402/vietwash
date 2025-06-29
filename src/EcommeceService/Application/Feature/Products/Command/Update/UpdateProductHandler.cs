using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using Mediator;
using System.Data.Common;

namespace Application.Feature.Products.Command.Update
{
	public class UpdateProductHandler(IUnitOfWork unitOfWork)
		: IRequestHandler<UpdateProductCommand, Result>
	{
		public async ValueTask<Result> Handle(
			UpdateProductCommand request,
			CancellationToken cancellationToken
			)
		{
			Product? existingProduct = await unitOfWork
				.Repository<Product>()
				.FindByConditionAsync(
					s => s.Id == request.ProductId && !s.Disable,
					cancellationToken
				);
			if (existingProduct == null)
			{
				return Result.Failure(
					new NotFoundError(
						"Product not found",
						Messager
							.Create<Product>()
							.Message(MessageType.Found)
							.Negative()
							.BuildMessage()
					)
				);
			}

			existingProduct.FromModel(request.Product);
			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
					cancellationToken
				);

				await unitOfWork.Repository<Product>().UpdateAsync(existingProduct);
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
