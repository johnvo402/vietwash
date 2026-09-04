using Contracts.Application.Common;

namespace Application.Features.Common.Projections
{
    public class NotificationProjection : BaseResponse
    {
        public string Title { get; set; } = default!;
        public string? Content { get; set; }
        public string? ContentHtml { get; set; }
        public bool IsRead { get; set; } = false;
        public Dictionary<string, string>? Data { get; set; }
    }
}
