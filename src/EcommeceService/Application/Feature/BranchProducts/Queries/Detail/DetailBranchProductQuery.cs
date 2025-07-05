using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.BranchProducts.Queries.Detail
{
    public class DetailBranchProductQuery : IRequest<Result<DetailBranchProductResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long Id { get; set; }
    }
}
