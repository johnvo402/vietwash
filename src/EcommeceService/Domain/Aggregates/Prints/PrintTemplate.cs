using Shared.Kernel.Common;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Prints
{
	public class PrintTemplate : AggregateRoot
	{
		public string Name { get; set; } = string.Empty;
		public string HtmlTemplate { get; set; } = string.Empty;

		protected override bool TryApplyDomainEvent(INotification domainEvent)
		{
			throw new NotImplementedException();
		}
	}
}
