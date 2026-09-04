namespace Application.Feature.Reports.Queries.ServiceOrderReport
{
    public class ServiceRevenueReportResponse
    {
        public long ServiceId { get; set; }
        public string ServiceName { get; set; } = default!;
        public long UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public int TotalOrderCount { get; set; }
        public decimal TotalNetRevenue { get; set; } // Tổng doanh thu thuần
        public decimal TotalDiscount { get; set; } // Tổng giảm giá
        public decimal TotalRevenue { get; set; } // Tổng doanh thu sau giảm giá
    }
}
