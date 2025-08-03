namespace Application.Feature.Reports.RevenueReport
{
    public class RevenueReportResponse
    {
        public DateOnly Date { get; set; }

        public long BranchId { get; set; }
        public int OrderQuantity { get; set; } //số lượng đơn hàng
        public int CustomerQuantity { get; set; } //số lượng khách hàng
        public decimal TotalRevenue { get; set; } // Tổng doanh thu
        public decimal TotalDiscount { get; set; } // Tổng giảm giá
        public decimal TotalNetRevenue { get; set; } // Tổng doanh thu thuần
        public decimal AverageRevenuePerOrder { get; set; } //trung bình doanh thu 1 đơn hàng
        public decimal AverageRevenuePerCustomer { get; set; } //trung bình doanh thu 1 khách hàng
    }
}
