using Ardalis.GuardClauses;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Products.Events;
using Domain.Aggregates.Services;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Products
{
    public class BranchProduct : AggregateRoot
    {
        public long BranchId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Sku { get; set; }
        public string? Image { get; set; }
        public decimal CapitalPrice { get; set; }
        public ActivationStatus Status { get; set; }
        public long CategoryId { get; set; }
        public bool Disable { get; set; } = default!;
        public ICollection<UnitRelation> UnitRelations { get; set; } = [];
        public ICollection<ProductSupplying> ProductSupplyings { get; set; } = [];
        public Category Category { get; set; } = default!;

        public BranchProduct(
            long branchId,
            string name,
            string sku,
            ActivationStatus status,
            decimal capitalPrice,
            long categoryId,
            string? description = null,
            string? image = null
        )
        {
            BranchId = Guard.Against.NegativeOrZero(branchId, nameof(branchId));
            Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Description = description?.Trim();
            Sku = Guard.Against.NullOrWhiteSpace(sku, nameof(sku));
            Image = image;
            Status = Guard.Against.EnumOutOfRange(status, nameof(status));
            CapitalPrice = Guard.Against.Negative(capitalPrice, nameof(capitalPrice));
            CategoryId = Guard.Against.Negative(categoryId, nameof(categoryId));
        }

        public void BranchProductCreateEvent() =>
            Emit(new BranchProductCreateEvent() { BranchProduct = this });

        public void Update(
            long? branchId = null,
            string? name = null,
            string? description = null,
            string? sku = null,
            string? image = null,
            decimal? capitalPrice = null,
            long? categoryId = null,
            ActivationStatus? status = null,
            bool? disable = null
        )
        {
            if (branchId.HasValue)
                BranchId = branchId.Value;

            if (!string.IsNullOrWhiteSpace(name))
                Name = name.Trim();

            if (description != null)
                Description = description.Trim();

            if (!string.IsNullOrWhiteSpace(sku))
                Sku = sku.Trim();

            if (image != null)
                Image = image;

            if (status.HasValue)
                Status = status.Value;

            if (disable.HasValue)
                Disable = disable.Value;
            if (capitalPrice != null)
                CapitalPrice = (long)capitalPrice;
            if (categoryId != null)
                CategoryId = (long)categoryId;
        }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
