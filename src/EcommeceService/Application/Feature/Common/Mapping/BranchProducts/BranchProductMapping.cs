using Application.Feature.Common.Projections.BranchProducts;
using Application.Features.Common.Mapping.Users;
using Domain.Aggregates.Products;
using Domain.Aggregates.Users;

namespace Application.Feature.Common.Mapping.BranchProducts
{
    public static class BranchProductMapping
    {
        public static BranchProductProjection ToBranchProductProjection(
            this BranchProduct branchProduct
        )
        {
            var result = new BranchProductProjection();
            result.MappingFrom(branchProduct);
            return result;
        }
    }
}
