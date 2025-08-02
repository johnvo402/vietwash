
namespace Application.Features.Report.FinanceReport
{
    public class FinancialReportResponse
    {
        public decimal TotalStockImport { get; set; }//tổng thu phiếu nhập hàng
        public decimal TotalStockExport { get; set; }//tổng chi phiếu xuất hàng
        public decimal TotalOtherIncome { get; set; }//tổng thu khác
        public decimal TotalOtherSpend { get; set; }//tổng chi khác
    }
}
