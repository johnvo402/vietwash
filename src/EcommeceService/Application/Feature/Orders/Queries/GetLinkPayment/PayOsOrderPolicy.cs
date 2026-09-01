namespace Application.Feature.Orders.Queries.GetLinkPayment;

public static class PayOsOrderPolicy
{
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
}
