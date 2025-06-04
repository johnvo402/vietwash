using AutoMapper;
using Domain.Aggregates.Suppliers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Suppliers.Command.Create
{
	public class CreateSupplierMapping : Profile
	{
		public CreateSupplierMapping()
		{
			CreateMap<CreateSupplierCommand, Supplier>();
		}
	}
}
