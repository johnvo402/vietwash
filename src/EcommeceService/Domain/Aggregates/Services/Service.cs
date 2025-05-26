using Domain.Aggregates.Orders;
using Domain.Aggregates.Services.Enums;
using Domain.Aggregates.Tariffs;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Services
{
    public class Service : AggregateRoot
    {
        public long CategoryId { get; set; } = default!;
		public long BranchId { get; set; } = default!;
		public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool Disable { get; set; } = default!;
		public string? Slug { get; set; }
        public TypeStatus Type { get; set; } = default!; // combo hay service thường
		public ServiceStatus Status { get; set; } = default!;
		public Category Category { get; set; } = default!;
		public ICollection<OrderItem> OrderItems { get; set; } = [];
        public ICollection<UnitRelation> UnitRelations { get; set; } = [];
        public ICollection<GroupService> GroupServices { get; set; } = [];
        public ICollection<ServicePriceTariffHistory> ServicePriceTariffHistories { get; set; } = [];


		protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
