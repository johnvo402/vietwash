using System.Data.Common;
using Application.Common.Errors;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using Domain.Aggregates.Products.Specifications;
using Mediator;

namespace Application.Feature.BranchProducts.Command.Update
{
    public class UpdateBranchProductHandler(
        IUnitOfWork unitOfWork,
        IMediaUpdateService mediaUpdateService
    ) : IRequestHandler<UpdateBranchProductCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateBranchProductCommand request,
            CancellationToken cancellationToken
        )
        {
            BranchProduct? existingBranchProduct = await unitOfWork
                .DynamicReadOnlyRepository<BranchProduct>()
                .FindByConditionAsync(
                    new GetBranchProductWithIncludeByIdSpecification(request.BranchProductId),
                    cancellationToken
                );
            if (existingBranchProduct == null)
            {
                return Result.Failure(
                    new NotFoundError(
                        "Branch product not found",
                        Messager
                            .Create<BranchProduct>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }

            string? oldImage = existingBranchProduct.Image;

            existingBranchProduct.FromUpdateModel(request.BranchProduct);

            string? newImage = request.BranchProduct.Image;
            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );

                await unitOfWork.Repository<BranchProduct>().UpdateAsync(existingBranchProduct);

                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                if (!string.IsNullOrEmpty(oldImage))
                {
                    await mediaUpdateService.DeleteAvatarAsync(oldImage);
                }

                return Result.Success();
            }
            catch
            {
                if (!string.IsNullOrEmpty(newImage))
                {
                    await mediaUpdateService.DeleteAvatarAsync(newImage);
                }
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
