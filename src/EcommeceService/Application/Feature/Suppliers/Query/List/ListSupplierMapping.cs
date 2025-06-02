using Application.Feature.Common.Projections.Suppliers;
using AutoMapper;
using Domain.Aggregates.Suppliers;

namespace Application.Feature.Suppliers.Query.List
{
	public class ListSupplierMapping : Profile
	{
		public ListSupplierMapping()
		{
			CreateMap<Supplier, SupplierProjection>();
			CreateMap<Supplier, ListSupplierResponse>();
		}
	}
}
