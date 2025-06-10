using Application.Feature.Common.Projections.Suppliers;
using AutoMapper;
using Domain.Aggregates.Suppliers;

namespace Application.Feature.Suppliers.Query.Detail
{
    public class GetSupplierDetailMapping : Profile
    {
        public GetSupplierDetailMapping()
        {
            CreateMap<Supplier, GetSupplierDetailResponse>()
                .IncludeBase<Supplier, SupplierProjection>();
        }
    }
}
