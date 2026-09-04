namespace Application.Features.Common.Projections
{
    public class NotificationModel
    {
        public string? MessageId { get; set; }
        public string TemplateId { get; set; } = default!;
        public List<string> UserIds { get; set; } = default!;
        public Dictionary<string, string>? Parameters { get; set; }
        public Dictionary<string, string>? Data { get; set; }
        public string Time { get; set; } = default!;
    }
}
