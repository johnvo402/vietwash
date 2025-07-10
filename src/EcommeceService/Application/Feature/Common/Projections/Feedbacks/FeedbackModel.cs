
namespace Application.Feature.Common.Projections.Feedbacks
{
	public class FeedbackModel
	{
		public long BranchId { get; set; }
		public long CustomerId { get; set; }
		public long ServiceId { get; set; }
		public int Rating { get; set; }
		public string Comment { get; set; } = default!;
	}
}
