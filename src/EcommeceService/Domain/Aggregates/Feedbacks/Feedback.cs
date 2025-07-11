using Ardalis.GuardClauses;
using Domain.Aggregates.Users;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Feedbacks
{
	public class Feedback : BaseEntity<long>
	{
		public long BranchId { get; set; }
		public long? CustomerId { get; set; }
		public long? StaffId { get; set; }
		public long ServiceId { get; set; } 
		public int? Rating { get; set; }
		public string Comment { get; set; } = default!;
		public int Likes { get; set; }
		public int Dislikes { get; set; }
		public long? ParentId { get; set; }   // null nếu là feedback gốc
		public Feedback? Parent { get; set; }
		public ICollection<Feedback> Replies { get; set; } = [];
		public User? Customer { get; set; }
		public User? Staff { get; set; }
		public bool Disable { get; set; } = false;
		public ICollection<FeedbackReaction> Reactions { get; set; } = [];

		public Feedback() { }

		public Feedback(
			long branchId,
			long serviceId,
			string comment,
			long? customerId = null,
			int? rating = null,
			long? staffId = null,
			long? parentId = null
		)
		{
			BranchId = Guard.Against.NegativeOrZero(branchId);
			ServiceId = Guard.Against.NegativeOrZero(serviceId);
			Comment = Guard.Against.NullOrWhiteSpace(comment);

			// Nếu là Feedback gốc từ Customer
			if (parentId == null)
			{
				CustomerId = Guard.Against.NegativeOrZero(customerId ?? 0, nameof(customerId));
				Rating = Guard.Against.NegativeOrZero(rating ?? 0, nameof(rating));
				StaffId = null;
			}
			// Nếu là Reply từ Staff
			else
			{
				ParentId = Guard.Against.NegativeOrZero(parentId.Value, nameof(parentId));
				StaffId = Guard.Against.NegativeOrZero(staffId ?? 0, nameof(staffId));
				CustomerId = null;
				Rating = null;
			}
			Likes = 0;
			Dislikes = 0;
		}

		public void Update(
			long? branchId,
			long? serviceId,
			string? comment,
			long? customerId = null,
			int? rating = null
		)
		{
			if (branchId.HasValue && branchId.Value > 0) BranchId = branchId.Value;
			if (serviceId.HasValue && serviceId.Value > 0) ServiceId = serviceId.Value;
			if (!string.IsNullOrWhiteSpace(comment)) Comment = comment;
			if (customerId.HasValue && customerId.Value > 0) CustomerId = customerId.Value;
			if (rating.HasValue) Rating = rating.Value;
		}
	}
}
