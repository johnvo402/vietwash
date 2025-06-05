using Application.Feature.Common.Projections.Suppliers;
using AutoMapper;


namespace Application.Feature.Common.Mapping.Suppliers
{
    public class SupplierMapping : Profile
    {
        public SupplierMapping()
        {
            CreateMap<SupplierModel, SupplierMapping>();

        }
    }
}
