using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Suppliers;
using Domain.Aggregates.Services;
using Domain.Aggregates.Suppliers;

namespace Application.Feature.Common.Mapping.Services
{
    public static class ServiceMapping
    {
        public static SupplierProjection ToSupplierProjection(this Supplier supplier)
        {
            var response = new SupplierProjection();
            response.MappingFrom(supplier);
            return response;
        }
    }
}
