using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Events.CreateEInvoiceEvents
{
    public class CreateEInvoiceEvent
        : PubSubBasePayload<EInvoiceOrderMessage>,
            IRequest<PubSubResponse<CreateEInvoiceEvent>>;

    public class EInvoiceOrderMessage
    {
        public long OrderId { get; set; }
        public string OrderCode { get; set; } = default!;
        public DateTimeOffset CompletedAt { get; set; }
        public string CustomerName { get; set; } = default!;
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public int Vat { get; set; }
        public decimal VatAmount { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public List<EInvoiceOrderItemMessage> Items { get; set; } = new();
    }

    public class EInvoiceOrderItemMessage
    {
        public string ServiceName { get; set; } = default!;
        public string? UnitRelationName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
