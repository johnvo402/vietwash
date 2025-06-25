using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Mediator;
using Domain.Aggregates.Products;
using Domain.Aggregates.Products.Specifications;

namespace Application.Feature.Products.Queries.Detail
{
	public class GetProductDetailHandler(IUnitOfWork unitOfWork)
		: IRequestHandler<GetProductDetailQuery, Result<GetProductDetailResponse>>
	{
		public async ValueTask<Result<GetProductDetailResponse>> Handle(
			GetProductDetailQuery query,
			CancellationToken cancellationToken
		)
		{
			var product = await unitOfWork
				.DynamicReadOnlyRepository<Product>()
				.FindByConditionAsync(
					new GetProductWithIncludeByIdSpecification(query.ProductId),
					cancellationToken
				);
			if (product == null)
			{
				return Result<GetProductDetailResponse>.Failure(
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

			var response = product.ToCreateUserResponse();
			return Result<GetProductDetailResponse>.Success(response);
		}
	}
}
