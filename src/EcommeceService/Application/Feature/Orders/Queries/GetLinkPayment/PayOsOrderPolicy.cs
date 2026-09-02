namespace Application.Feature.Orders.Queries.GetLinkPayment;

public static class PayOsOrderPolicy
{
    public const string PaymentLinkNotFoundCode = "231";

    public static long GetOrderCode(long orderId)
    {
        if (orderId <= 0)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        return orderId;
    }

    public static string GetCheckoutUrl(string paymentLinkId)
    {
        if (string.IsNullOrWhiteSpace(paymentLinkId))
            throw new ArgumentException("Payment link id is required.", nameof(paymentLinkId));
        return $"https://pay.payos.vn/web/{paymentLinkId}";
    }

    public static bool TryGetAmount(decimal total, out int amount)
    {
        if (total <= 0 || total > int.MaxValue || decimal.Truncate(total) != total)
        {
            amount = default;
            return false;
        }

        amount = decimal.ToInt32(total);
        return true;
    }

    public static string GetDescription(string? orderCode, long orderId)
    {
        string normalized = new(
            (orderCode ?? string.Empty)
                .ToUpperInvariant()
                .Where(character => character is >= 'A' and <= 'Z' or >= '0' and <= '9')
                .Take(22)
                .ToArray()
        );
        return $"VW {(normalized.Length == 0 ? orderId.ToString() : normalized)}";
    }

    public static OrderPaymentLinkState GetState(string? providerState) =>
        providerState?.Trim().ToUpperInvariant() switch
        {
            "PENDING" => OrderPaymentLinkState.Pending,
            "PROCESSING" => OrderPaymentLinkState.Processing,
            "PAID" => OrderPaymentLinkState.Paid,
            "CANCELLED" => OrderPaymentLinkState.Cancelled,
            _ => OrderPaymentLinkState.Unknown,
        };
}

public enum OrderPaymentLinkState
{
    Pending,
    Processing,
    Paid,
    Cancelled,
    Unknown,
}
