using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Services.Queries.Detail;
using Application.Features.Common.Mapping.Users;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using Domain.Aggregates.Products.Specifications;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Mediator;

namespace Application.Feature.BranchProducts.Queries.Detail
{
    public class DetailBranchProductHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DetailBranchProductQuery, Result<DetailBranchProductResponse>>
    {
        public async ValueTask<Result<DetailBranchProductResponse>> Handle(
            DetailBranchProductQuery request,
            CancellationToken cancellationToken
        )
        {
            var response = await unitOfWork
                .DynamicReadOnlyRepository<BranchProduct>()
                .FindByConditionAsync(
                    new GetBranchProductWithIncludeByIdSpecification(request.Id),
                    x => x.ToDetailBranchProductResponse(),
                    cancellationToken
                );
            if (response == null)
            {
                return Result<DetailBranchProductResponse>.Failure(
                    new NotFoundError(
                        "Product not fount",
                        Messager
                            .Create<BranchProduct>()
                            .Message(MessageType.Found)
                            .Negative()
                            .Build()
                    )
                );
            }
            if (!string.IsNullOrEmpty(response.CreatedBy) && response.CreatedBy != "SYSTEM")
            {
                var createdUser = await unitOfWork
                    .DynamicReadOnlyRepository<User>()
                    .FindByConditionAsync(
                        new GetUserByIdWithoutIncludeSpecification(long.Parse(response.CreatedBy)),
                        x => x.UserDTOResponse(),
                        cancellationToken
                    );

                response.CreatedUser = createdUser;
            }
            if (!string.IsNullOrEmpty(response.UpdatedBy) && response.UpdatedBy != "SYSTEM")
            {
                var updatedUser = await unitOfWork
                    .DynamicReadOnlyRepository<User>()
                    .FindByConditionAsync(
                        new GetUserByIdWithoutIncludeSpecification(long.Parse(response.UpdatedBy)),
                        x => x.UserDTOResponse(),
                        cancellationToken
                    );

                response.UpdatedUser = updatedUser;
            }

            return Result<DetailBranchProductResponse>.Success(response);
        }
    }
}
