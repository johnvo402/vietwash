using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Contracts.Utils;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Mediator;
using Net.payOS;
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
            List<ItemData> itemDataList = order
                .Items.Select(i => new ItemData(i.Name, i.Quantity, i.Amount))
                .ToList();
            var paymentLinkRequest = new PaymentData(
                orderCode: long.Parse(Generator.GenerateCode(6)),
                amount: order.Amount,
                description: $"Don hang {order.Code}",
                items: itemDataList,
                returnUrl: request.ReturnUrl,
                cancelUrl: request.ReturnUrl
            );
            CreatePaymentResult response = await payOS.createPaymentLink(paymentLinkRequest);

            return Result<CreatePaymentResult>.Success(response);
        }
    }
}
