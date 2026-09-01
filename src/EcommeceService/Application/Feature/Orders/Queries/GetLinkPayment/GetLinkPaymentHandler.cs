using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Specifications;
using Mediator;
using Net.payOS;
using Net.payOS.Errors;
using Net.payOS.Types;

namespace Application.Feature.Orders.Queries.GetLinkPayment
{
    public class GetLinkPaymentHandler(PayOS payOS, IUnitOfWork unitOfWork)
        : IRequestHandler<GetLinkPaymentQuery, Result<CreatePaymentResult>>
    {
        public async ValueTask<Result<CreatePaymentResult>> Handle(
            GetLinkPaymentQuery request,
            CancellationToken cancellationToken
        )
        {
            var order = await unitOfWork
                .DynamicReadOnlyRepository<Order>()
                .FindByConditionAsync(
                    new GetOrderByIdSpecification(request.OrderId),
                    GetLinkPaymentMapping.Selector(),
                    cancellationToken
                );
            if (order == null)
            {
                return Result<CreatePaymentResult>.Failure(
                    new NotFoundError(
                        "Order not found",
                        Messager
                            .Create<Order>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    )
                );
            }
            if (order.Status != OrderStatus.Processed)
                return Result<CreatePaymentResult>.Failure(
                    new BadRequestError(
                        "Only processed orders are eligible for payment.",
                        Messager.Create<Order>().Message(MessageType.Valid).Negative().BuildMessage()
                    )
                );
            if (!PayOsOrderPolicy.TryGetAmount(order.Amount, out int amount))
                return Result<CreatePaymentResult>.Failure(
                    new BadRequestError(
                        "Order total must be a positive whole VND amount supported by payOS.",
                        Messager.Create<Order>().Message(MessageType.Valid).Negative().BuildMessage()
                    )
                );

            var itemDataList = new List<ItemData>();
            foreach (OrderPaymentItem item in order.Items)
            {
                if (!PayOsOrderPolicy.TryGetAmount(item.Amount, out int itemAmount))
                    return Result<CreatePaymentResult>.Failure(
                        new BadRequestError(
                            "Order item price is not supported by payOS.",
                            Messager
                                .Create<Order>()
                                .Message(MessageType.Valid)
                                .Negative()
                                .BuildMessage()
                        )
                    );
                itemDataList.Add(new ItemData(item.Name, item.Quantity, itemAmount));
            }
            var paymentLinkRequest = new PaymentData(
                orderCode: PayOsOrderPolicy.GetOrderCode(order.Id),
                amount: amount,
                description: $"Don hang {order.Code}",
                items: itemDataList,
                returnUrl: request.ReturnUrl,
                cancelUrl: request.ReturnUrl
            );
            CreatePaymentResult response;
            try
            {
                response = await payOS.createPaymentLink(paymentLinkRequest);
            }
            catch (PayOSError)
            {
                PaymentLinkInformation existing = await payOS.getPaymentLinkInformation(order.Id);
                response = new CreatePaymentResult(
                    bin: string.Empty,
                    accountNumber: string.Empty,
                    amount: existing.amount,
                    description: $"Don hang {order.Code}",
                    orderCode: existing.orderCode,
                    currency: "VND",
                    paymentLinkId: existing.id,
                    status: existing.status,
                    expiredAt: null,
                    checkoutUrl: PayOsOrderPolicy.GetCheckoutUrl(existing.id),
                    qrCode: string.Empty
                );
            }

            return Result<CreatePaymentResult>.Success(response);
        }
    }
}
