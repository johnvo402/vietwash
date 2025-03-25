namespace Application.Feature.Statistics.Queries.TopService
{
    public class GetTopServiceResponse
    {
        public string? ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string? Description { get; set; }
        public int? UsageCount { get; set; }
        public decimal? TotalRevenue { get; set; }
    }
}
