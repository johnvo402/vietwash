using Application.Feature.Common.Projections.Products;
using Contracts.ApiWrapper;
using Mediator;


namespace Application.Feature.Products.Command.Create
{
	public class CreateProductCommand : ProductModel, IRequest<Result>;
}
