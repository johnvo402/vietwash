
namespace Application.Feature.Reports.FinancialReport
{

    public class FinancialReportResponse
    {

        public decimal TotalRevenue { get; set; } // Tổng doanh thu
        public decimal CancelValue { get; set; } // Tổng doanh thu
        public decimal TotalDiscount { get; set; } // Tổng giảm giá
        public decimal TotalPoint { get; set; } //Tổng tiền giảm từ điểm
        public decimal TotalNetRevenue { get; set; } // Tổng doanh thu thuần = TotalRevenue-TotalDiscount


    }

}
