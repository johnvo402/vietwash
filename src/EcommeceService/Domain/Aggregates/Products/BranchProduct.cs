using Ardalis.GuardClauses;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Services;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Products
{
    public class BranchProduct : BaseEntity<long>
    {
        public long BranchId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public string? Image { get; set; }
        public ActivationStatus Status { get; set; }
        public bool Disable { get; set; } = default!;
        public ICollection<UnitRelation> UnitRelations { get; set; } = [];

        public BranchProduct(
            long branchId,
            string name,
            string sku,
            ActivationStatus status,
            string? description = null,
            string barcode = null,
            string? image = null
        )
        {
            BranchId = Guard.Against.NegativeOrZero(branchId, nameof(branchId));
            Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Description = description?.Trim();
            Sku = Guard.Against.NullOrWhiteSpace(sku, nameof(sku));
            Barcode = Guard.Against.NullOrWhiteSpace(barcode, nameof(barcode));
            Image = image;
            Status = Guard.Against.EnumOutOfRange(status, nameof(status));
        }

        public void Update(
            long? branchId = null,
            string? name = null,
            string? description = null,
            string? sku = null,
            string? barcode = null,
            string? image = null,
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

            if (!string.IsNullOrWhiteSpace(sku))
                Barcode = barcode.Trim();

            if (image != null)
                Image = image;

            if (status.HasValue)
                Status = status.Value;

            if (disable.HasValue)
                Disable = disable.Value;
        }
    }
}
