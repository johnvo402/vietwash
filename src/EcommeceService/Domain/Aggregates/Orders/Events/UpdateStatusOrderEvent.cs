using Domain.Aggregates.Funds;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Orders.Events
{
	public class UpdateStatusOrderEvent : INotification
	{
		public string TypeId { get; set; } = default!;
		public string BehaviorId { get; set; } = default!;
		public decimal Amount { get; set; } = default!;
		public PaymentMethod PaymentMethod { get; set; } = default!;
		public Ulid? ReferenceId { get; set; }
	}
}
