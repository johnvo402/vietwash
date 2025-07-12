using Domain.Aggregates.Products;

namespace Application.Feature.BranchProducts.Queries.Detail
{
    public static class DetailBranchProductMapping
    {
        public static DetailBranchProductResponse ToDetailBranchProductResponse(
            this BranchProduct branchProduct
        )
        {
            var result = new DetailBranchProductResponse();
            result.MappingFrom(branchProduct);
            return result;
        }
    }
}
