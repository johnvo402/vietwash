using Application.Feature.Common.Projections.Suppliers;
using AutoMapper;
using Domain.Aggregates.Suppliers;


namespace Application.Feature.Suppliers.Command.Update
{
	public class UpdateSupplierMapping : Profile
	{
		public UpdateSupplierMapping()
		{
			CreateMap<UpdateSupplierCommand, Supplier>();
			CreateMap<SupplierModel, Supplier>();
			CreateMap<Supplier, UpdateSupplierResponse>();
		}
	}
}
