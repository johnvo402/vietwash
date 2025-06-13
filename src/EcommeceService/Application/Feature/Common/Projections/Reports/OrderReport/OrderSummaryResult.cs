namespace Application.Feature.Common.Projections.Reports.OrderReport
{
    public class OrderSummaryResult
    {
        public long OrderId { get; set; }
        public string Code { get; set; }
        public long BranchId { get; set; }
        public long CustomerId { get; set; }
        public int OrderItemCount { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset OrderDate { get; set; }
    }
}
