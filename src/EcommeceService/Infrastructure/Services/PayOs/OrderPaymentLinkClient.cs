using Application.Feature.Orders.Queries.GetLinkPayment;
using Net.payOS;
using Net.payOS.Types;

namespace Infrastructure.Services.PayOs;

public sealed class OrderPaymentLinkClient(PayOS payOS) : IOrderPaymentLinkClient
{
    public Task<CreatePaymentResult> CreatePaymentLinkAsync(PaymentData paymentData) =>
        payOS.createPaymentLink(paymentData);

    public Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long orderId) =>
        payOS.getPaymentLinkInformation(orderId);

    public Task<PaymentLinkInformation> CancelPaymentLinkAsync(
        long orderId,
        string cancellationReason
    ) => payOS.cancelPaymentLink(orderId, cancellationReason);
}

public sealed class PayOsWebhookVerifier(PayOS payOS) : IOrderPaymentWebhookVerifier
{
    public WebhookData Verify(WebhookType request) => payOS.verifyPaymentWebhookData(request);
}

public sealed class UnavailableOrderPaymentLinkClient : IOrderPaymentLinkClient
{
    public Task<CreatePaymentResult> CreatePaymentLinkAsync(PaymentData paymentData) =>
        Task.FromException<CreatePaymentResult>(new PayOsUnavailableException());

    public Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long orderId) =>
        Task.FromException<PaymentLinkInformation>(new PayOsUnavailableException());

    public Task<PaymentLinkInformation> CancelPaymentLinkAsync(
        long orderId,
        string cancellationReason
    ) => Task.FromException<PaymentLinkInformation>(new PayOsUnavailableException());
}

public sealed class UnavailablePayOsWebhookVerifier : IOrderPaymentWebhookVerifier
{
    public WebhookData Verify(WebhookType request) => throw new PayOsUnavailableException();
}
