using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using Domain.Aggregates.Products.Specifications;
using Mediator;
using System.Data.Common;

namespace Application.Feature.Products.Command.Update
{
	public class UpdateProductHandler(
		IUnitOfWork unitOfWork,
		IMediaUpdateService mediaUpdateService
	)
		: IRequestHandler<UpdateProductCommand, Result>
	{
		public async ValueTask<Result> Handle(
			UpdateProductCommand request,
			CancellationToken cancellationToken
			)
		{
			Product? existingProduct = await unitOfWork
				.DynamicReadOnlyRepository<Product>()
				.FindByConditionAsync(
					new GetProductWithIncludeByIdSpecification(request.ProductId),
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
			string? oldProductImage = existingProduct.Image;
			existingProduct.UpdateFromModel(request.Product);
			string? newProductImage = request.Product.Image;
			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
					cancellationToken
				);

				await unitOfWork.Repository<Product>().UpdateAsync(existingProduct);
				await unitOfWork.SaveAsync(cancellationToken);
				await unitOfWork.CommitAsync(cancellationToken);

				if (!string.IsNullOrEmpty(oldProductImage))
				{
					await mediaUpdateService.DeleteAvatarAsync(oldProductImage);
				}

				return Result.Success();
			}
			catch
			{
				if (!string.IsNullOrEmpty(newProductImage))
				{
					await mediaUpdateService.DeleteAvatarAsync(newProductImage);
				}
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
