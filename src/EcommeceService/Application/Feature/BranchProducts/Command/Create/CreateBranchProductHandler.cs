using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Products;
using Mediator;
using System.Data.Common;

namespace Application.Feature.BranchProducts.Command.Create
{
	public class CreateBranchProductHandler(
		IUnitOfWork unitOfWork,
		IMediaUpdateService mediaUpdateService
	) : IRequestHandler<CreateBranchProductCommand, Result>
	{
		public async ValueTask<Result> Handle(
			CreateBranchProductCommand request,
			CancellationToken cancellationToken
		)
		{
			BranchProduct mappingBranchProduct = request.ToEntity();
			string? image = null;
			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
					cancellationToken
				);

				BranchProduct branchProduct = await unitOfWork
					.Repository<BranchProduct>()
					.AddAsync(mappingBranchProduct, cancellationToken);
				image = branchProduct.Image;

				await unitOfWork.SaveAsync(cancellationToken);
				await unitOfWork.CommitAsync(cancellationToken);
				return Result.Success();
			}
			catch (Exception)
			{
				if (!string.IsNullOrEmpty(image))
				{
					await mediaUpdateService.DeleteAvatarAsync(image);
				}
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
