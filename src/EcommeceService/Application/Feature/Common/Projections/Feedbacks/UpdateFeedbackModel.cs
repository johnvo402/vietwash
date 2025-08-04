namespace Application.Feature.Common.Projections.Feedbacks;

public class UpdateFeedbackModel
{
    public int? Rating { get; set; } = null;
    public string Comment { get; set; } = default!;
}
