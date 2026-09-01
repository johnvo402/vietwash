using Application.Feature.Common.Projections.Orders;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Orders.Command.Create
{
    public class CreateOrderCommand : IRequest<Result<CreateOrderResponse>>
    {
        public long CustomerId { get; set; }
        public long BranchId { get; set; }
        public long TariffId { get; set; }
        public string? VoucherCode { get; set; }
        public string? Note { get; set; } = string.Empty;
        public DateTimeOffset? DeliveryTime { get; set; }
        public List<OrderItemSelectionModel> OrderItems { get; set; } = [];
    }
}
