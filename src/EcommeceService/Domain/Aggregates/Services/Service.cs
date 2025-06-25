using Ardalis.GuardClauses;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Services.Enums;
using Domain.Aggregates.Tariffs;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Services
{
	public class Service : AggregateRoot
	{
		public string CategoryId { get; set; } = default!;
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
		public ICollection<ServicePriceTariffHistory> ServicePriceTariffHistories { get; set; } =
			[];

		public Service(
			string categoryId,
			long branchId,
			string name,
			TypeStatus type,
			ServiceStatus status,
			string? description = null,
			string? image = null
		)
		{
			CategoryId = Guard.Against.NullOrWhiteSpace(categoryId, nameof(categoryId));
			BranchId = Guard.Against.NegativeOrZero(branchId, nameof(branchId));
			Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
			Type = Guard.Against.EnumOutOfRange(type, nameof(type));
			Status = Guard.Against.EnumOutOfRange(status, nameof(status));
			Description = description;
			Image = image;
		}

		public void Update(
			string? categoryId = null,
			long? branchId = null,
			string? name = null,
			TypeStatus? type = null,
			ServiceStatus? status = null,
			string? description = null,
			string? image = null
		)
		{
			if (!string.IsNullOrWhiteSpace(categoryId))
				CategoryId = categoryId.Trim();

			if (branchId.HasValue)
				BranchId = branchId.Value;

			if (!string.IsNullOrWhiteSpace(name))
				Name = name.Trim();

			if (type.HasValue)
				Type = Guard.Against.EnumOutOfRange(type.Value, nameof(type));

			if (status.HasValue)
				Status = Guard.Against.EnumOutOfRange(status.Value, nameof(status));

			if (description != null)
				Description = description;

			if (image != null)
				Image = image;
		}

		protected override bool TryApplyDomainEvent(INotification domainEvent)
		{
			throw new NotImplementedException();
		}
	}
}
