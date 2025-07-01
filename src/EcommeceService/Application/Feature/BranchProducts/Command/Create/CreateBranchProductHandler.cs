using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Services.Command.Create;
using Contracts.ApiWrapper;
using Contracts.Utils;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using Mediator;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			string? Image = null;
			try
			{
				DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
					cancellationToken
				);

				BranchProduct branchProduct = await unitOfWork
					.Repository<BranchProduct>()
					.AddAsync(mappingBranchProduct, cancellationToken);
				Image = branchProduct.Image;

				await unitOfWork.SaveAsync(cancellationToken);
				await unitOfWork.CommitAsync(cancellationToken);
				return Result.Success();
			}
			catch (Exception)
			{
				if (!string.IsNullOrEmpty(Image))
				{
					await mediaUpdateService.DeleteAvatarAsync(Image);
				}
				await unitOfWork.RollbackAsync(cancellationToken);
				throw;
			}
		}
	}
}
