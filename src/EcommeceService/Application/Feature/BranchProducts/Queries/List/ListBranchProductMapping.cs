using System.Linq.Expressions;
using Application.Feature.Common.Mapping.BranchProducts;
using Domain.Aggregates.Products;

namespace Application.Feature.BranchProducts.Queries.List
{
    public static class ListBranchProductMapping
    {
        public static Expression<Func<BranchProduct, ListBranchProductResponse>> Selector() =>
            products => (ListBranchProductResponse)products.ToBranchProductProjection();
    }
}
