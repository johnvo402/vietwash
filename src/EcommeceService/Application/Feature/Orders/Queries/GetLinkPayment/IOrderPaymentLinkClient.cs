using Net.payOS.Types;

namespace Application.Feature.Orders.Queries.GetLinkPayment;

public interface IOrderPaymentLinkClient
{
    Task<CreatePaymentResult> CreatePaymentLinkAsync(PaymentData paymentData);

    Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long orderId);
}
