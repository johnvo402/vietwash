using Domain.Aggregates.Orders;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;
namespace Domain.Aggregates.Services
{
    public class Service : AggregateRoot
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool Disable { get; set; } = default!;
        public string CategoryId { get; set; } = default!;
        public virtual Category Category { get; set; } = default!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = [];
        public virtual ICollection<UnitRelation> UnitRelations { get; set; } = [];
        public virtual ICollection<GroupService> GroupServices { get; set; } = [];

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
