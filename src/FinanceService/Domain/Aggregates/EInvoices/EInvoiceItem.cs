using Shared.Kernel.Common;

namespace Domain.Aggregates.EInvoices
{
    public class EInvoiceItem : DefaultEntity
    {
        public long EInvoiceId { get; set; }

        public EInvoice EInvoice { get; set; } = null!;

        public string ServiceName { get; set; } = null!;

        public string? UnitRelationName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
