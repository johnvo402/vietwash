using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Products;
using Mediator;
using System.Data.Common;

namespace Application.Feature.Products.Command.Create
{
	public class CreateProductHandler(
		IUnitOfWork unitOfWork,
		IMediaUpdateService mediaUpdateService
		)
		: IRequestHandler<CreateProductCommand, Result>
	{
		public async ValueTask<Result> Handle(
			CreateProductCommand request,
			CancellationToken cancellationToken
		)
		{
			Product product = request.ToEntity();
			string? productImage = null;
			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
					cancellationToken
				);

				var response = await unitOfWork.Repository<Product>().AddAsync(product, cancellationToken);
				productImage = response.Image;
				await unitOfWork.SaveAsync(cancellationToken);
				await unitOfWork.CommitAsync(cancellationToken);

				return Result.Success();
			}
			catch
			{
				if (!string.IsNullOrEmpty(productImage))
				{
					await mediaUpdateService.DeleteAvatarAsync(productImage);
				}
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
