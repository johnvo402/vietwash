using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Command.Delete
{
	public record DeleteOrderCommand(Ulid OrderId) : IRequest;
}
