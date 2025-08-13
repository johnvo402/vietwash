namespace Application.Features.Common.Projections
{
    public class NotificationModel
    {
        public string TemplateId { get; set; }
        public List<string> UserIds { get; set; }
        public Dictionary<string, string>? Parameters { get; set; }
        public Dictionary<string, string>? Data { get; set; }
        public string Time { get; set; }
    }
}
