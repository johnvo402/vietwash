namespace Application.Feature.Statistics.Queries.SaleResult
{
    public class GetSaleResultResponse
    {
        public int NumberOrder { get; set; }
        public decimal Revenue { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal RevenueYesterday { get; set; }
        public float PercentageChangeDay { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public float PercentageChangeMonth { get; set; }
    }
}
