using Microsoft.EntityFrameworkCore;

namespace Domain.Functions
{
    [Keyless]
    public class CustomerRevenueResult
    {
        public long? CustomerId { get; set; }
        public string? CustomerCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DisplayName { get; set; }
        public string? AvtUrl { get; set; }
        public decimal Revenue { get; set; }
        public decimal CancelValue { get; set; }
        public decimal NetRevenue { get; set; }
        public int OrderSaleQuantity { get; set; }
        public int OrderCancelQuantity { get; set; }
    }
}
