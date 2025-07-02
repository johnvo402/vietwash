using Application.Feature.Common.Projections.BranchProducts;
using Contracts.ApiWrapper;
using Mediator;


namespace Application.Feature.BranchProducts.Command.Create;

public class CreateBranchProductCommand : BranchProductModel, IRequest<Result>;
