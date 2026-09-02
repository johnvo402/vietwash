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
