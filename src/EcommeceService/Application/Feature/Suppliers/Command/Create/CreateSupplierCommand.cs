using Application.Feature.Common.Projections.Suppliers;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Suppliers.Command.Create
{
	public class CreateSupplierCommand : SupplierModel, IRequest;
}
