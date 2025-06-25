using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Suppliers.Command.Delete;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using Domain.Aggregates.Suppliers;
using Mediator;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Products.Command.Delete
{
	public class DeleteProductHandler(IUnitOfWork unitOfWork)
		: IRequestHandler<DeleteProductCommand, Result>
	{
		public async ValueTask<Result> Handle(
			DeleteProductCommand request,
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

			existingProduct.Disable = true;

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
