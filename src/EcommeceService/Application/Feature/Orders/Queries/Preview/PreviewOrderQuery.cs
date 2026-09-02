using Application.Feature.Common.Projections.Orders;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Orders.Queries.Preview;

public sealed class PreviewOrderQuery : IRequest<Result<PreviewOrderResponse>>
{
    public long CustomerId { get; set; }
    public long BranchId { get; set; }
    public long TariffId { get; set; }
    public string? VoucherCode { get; set; }
    public List<OrderItemSelectionModel> OrderItems { get; set; } = [];
}

public sealed class PreviewOrderResponse
{
    public decimal Amount { get; init; }
    public decimal DiscountAmount { get; init; }
    public bool DiscountFixed { get; init; }
    public decimal DiscountValue { get; init; }
    public decimal NetBeforeVat { get; init; }
    public int VatPercent { get; init; }
    public decimal VatAmount { get; init; }
    public decimal Total { get; init; }
    public IReadOnlyList<PreviewOrderLine> OrderItems { get; init; } = [];
}

public sealed class PreviewOrderLine
{
    public long ServiceId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public long UnitRelationId { get; init; }
    public string UnitRelationName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal LineAmount { get; init; }
}
