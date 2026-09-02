using Net.payOS.Types;

namespace Application.Feature.Orders.Queries.GetLinkPayment;

public interface IOrderPaymentLinkClient
{
    Task<CreatePaymentResult> CreatePaymentLinkAsync(PaymentData paymentData);

    Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long orderId);

    Task<PaymentLinkInformation> CancelPaymentLinkAsync(long orderId, string cancellationReason);
}

public interface IOrderPaymentSettings
{
    bool IsEnabled { get; }

    string? ReturnUrl { get; }

    string? CancelUrl { get; }

    string? WebhookUrl { get; }
}

public interface IOrderPaymentWebhookVerifier
{
    WebhookData Verify(WebhookType request);
}

public sealed class PayOsUnavailableException()
    : InvalidOperationException("PayOS payment is not configured.");
