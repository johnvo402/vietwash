
namespace Application.Feature.Common.Projections.Feedbacks
{
	public class ReplyFeedbackModel
	{
		public long StaffId { get; set; }
		public long ParentId { get; set; }
		public string Comment { get; set; } = default!;
	}
}
