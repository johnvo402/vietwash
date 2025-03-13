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
        public Ulid CategoryId { get; set; } = default!;
        public Category Category { get; set; } = default!;
        public ICollection<OrderItem> OrderItems { get; set; } = [];
        public ICollection<UnitRelation> UnitRelations { get; set; } = [];
        public ICollection<GroupService> GroupServices { get; set; } = [];

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
