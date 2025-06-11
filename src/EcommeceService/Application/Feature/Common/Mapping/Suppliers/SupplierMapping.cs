using Application.Feature.Common.Projections.Suppliers;
using AutoMapper;
using Domain.Aggregates.Suppliers;


namespace Application.Feature.Common.Mapping.Suppliers
{
    public class SupplierMapping : Profile
    {
        public SupplierMapping()
        {
            CreateMap<SupplierModel, Supplier>();

        }
    }
}
