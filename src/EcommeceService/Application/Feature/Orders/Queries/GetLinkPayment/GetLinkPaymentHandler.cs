using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Specifications;
using Mediator;
using Net.payOS.Errors;
using Net.payOS.Types;
using Serilog;

namespace Application.Feature.Orders.Queries.GetLinkPayment;

public class GetLinkPaymentHandler(
    IOrderPaymentLinkClient paymentClient,
    IUnitOfWork unitOfWork,
    ICurrentAccount currentAccount,
    IOrderPaymentSettings? paymentSettings = null,
    ILogger? logger = null
) : IRequestHandler<GetLinkPaymentQuery, Result<CreatePaymentResult>>
{
    private readonly ILogger _logger = logger ?? Log.Logger;

    public async ValueTask<Result<CreatePaymentResult>> Handle(
        GetLinkPaymentQuery request,
        CancellationToken cancellationToken
    )
    {
        OrderPayment? order = await unitOfWork
            .DynamicReadOnlyRepository<Order>()
            .FindByConditionAsync(
                new GetOrderByIdSpecification(request.OrderId),
                GetLinkPaymentMapping.Selector(),
                cancellationToken
            );
        if (order is null)
            return Failure(
                new NotFoundError(
                    "Order not found",
                    Messager.Create<Order>().Message(MessageType.Found).Negative().BuildMessage()
                )
            );

        if (
            !OrderBranchAccess
                .FromSession(currentAccount.Session?.Branches)
                .IsAuthorized(order.BranchId)
        )
            return Failure(new ForbiddenError(Message.FORBIDDEN));

        if (order.Status != OrderStatus.Processed)
            return BadRequest("Only processed orders are eligible for payment.");

        if (paymentSettings?.IsEnabled != true)
            return BadRequest("PayOS payment is unavailable. Choose cash or try again later.");

        if (!PayOsOrderPolicy.TryGetAmount(order.Amount, out int amount))
            return BadRequest(
                "Order total must be a positive whole VND amount supported by PayOS."
            );

        PaymentLinkInformation? existing;
        try
        {
            existing = await paymentClient.GetPaymentLinkInformationAsync(order.Id);
        }
        catch (PayOSError error) when (error.Code == PayOsOrderPolicy.PaymentLinkNotFoundCode)
        {
            existing = null;
        }
        catch (Exception error)
        {
            LogProviderFailure(order.Id, "get", error.GetType().Name, (error as PayOSError)?.Code);
            return BadRequest("PayOS could not check the payment link. Please try again later.");
        }

        if (existing is not null)
            return HandleExistingPaymentLink(order, amount, existing);

        string description = PayOsOrderPolicy.GetDescription(order.Code, order.Id);
        var paymentLinkRequest = new PaymentData(
            orderCode: PayOsOrderPolicy.GetOrderCode(order.Id),
            amount: amount,
            description: description,
            items: BuildInformationalItems(order.Items, amount),
            returnUrl: paymentSettings.ReturnUrl!.Trim(),
            cancelUrl: paymentSettings.CancelUrl!.Trim()
        );

        try
        {
            CreatePaymentResult created = await paymentClient.CreatePaymentLinkAsync(
                paymentLinkRequest
            );
            if (created.orderCode != order.Id || created.amount != amount)
            {
                LogProviderFailure(order.Id, "create", "ProviderResponseMismatch", null);
                return BadRequest("PayOS returned payment data that does not match this order.");
            }

            return PayOsOrderPolicy.GetState(created.status) switch
            {
                OrderPaymentLinkState.Pending
                or OrderPaymentLinkState.Processing
                or OrderPaymentLinkState.Paid => Result<CreatePaymentResult>.Success(created),
                OrderPaymentLinkState.Cancelled => CancelledPaymentLink(),
                _ => UnknownPaymentState(order.Id, created.status, "create"),
            };
        }
        catch (Exception error)
        {
            LogProviderFailure(
                order.Id,
                "create",
                error.GetType().Name,
                (error as PayOSError)?.Code
            );
            return BadRequest("PayOS could not create the payment link. Please try again later.");
        }
    }

    private Result<CreatePaymentResult> HandleExistingPaymentLink(
        OrderPayment order,
        int amount,
        PaymentLinkInformation existing
    )
    {
        if (existing.orderCode != order.Id || existing.amount != amount)
        {
            LogProviderFailure(order.Id, "get", "ProviderResponseMismatch", null);
            return BadRequest("PayOS returned payment data that does not match this order.");
        }

        return PayOsOrderPolicy.GetState(existing.status) switch
        {
            OrderPaymentLinkState.Pending
            or OrderPaymentLinkState.Processing
            or OrderPaymentLinkState.Paid => Result<CreatePaymentResult>.Success(
                ToCreatePaymentResult(order, existing)
            ),
            OrderPaymentLinkState.Cancelled => CancelledPaymentLink(),
            _ => UnknownPaymentState(order.Id, existing.status, "get"),
        };
    }

    private static CreatePaymentResult ToCreatePaymentResult(
        OrderPayment order,
        PaymentLinkInformation existing
    ) =>
        new(
            bin: string.Empty,
            accountNumber: string.Empty,
            amount: existing.amount,
            description: PayOsOrderPolicy.GetDescription(order.Code, order.Id),
            orderCode: existing.orderCode,
            currency: "VND",
            paymentLinkId: existing.id,
            status: existing.status,
            expiredAt: null,
            checkoutUrl: PayOsOrderPolicy.GetCheckoutUrl(existing.id),
            qrCode: string.Empty
        );

    private static List<ItemData> BuildInformationalItems(
        IEnumerable<OrderPaymentItem> orderItems,
        int authoritativeAmount
    )
    {
        List<ItemData> items = [];
        decimal total = 0;
        foreach (OrderPaymentItem item in orderItems)
        {
            if (
                item.Quantity <= 0
                || !PayOsOrderPolicy.TryGetAmount(item.Amount, out int itemAmount)
            )
                return [];

            total += item.Amount * item.Quantity;
            items.Add(new ItemData(item.Name, item.Quantity, itemAmount));
        }

        return total == authoritativeAmount ? items : [];
    }

    private Result<CreatePaymentResult> UnknownPaymentState(
        long orderId,
        string? status,
        string operation
    )
    {
        LogProviderFailure(orderId, operation, "UnknownProviderState", status);
        return BadRequest("PayOS returned an unsupported payment state. Please try again later.");
    }

    private static Result<CreatePaymentResult> CancelledPaymentLink() =>
        BadRequest(
            "This PayOS payment link was cancelled. Choose cash or cancel the business order."
        );

    private void LogProviderFailure(
        long orderId,
        string operation,
        string providerErrorType,
        string? providerErrorCode
    ) =>
        _logger.Warning(
            "PayOS operation failed for Order {OrderId}: operation {Operation}, error type {ProviderErrorType}, code {ProviderErrorCode}",
            orderId,
            operation,
            providerErrorType,
            providerErrorCode
        );

    private static Result<CreatePaymentResult> BadRequest(string message) =>
        Failure(
            new BadRequestError(
                message,
                Messager.Create<Order>().Message(MessageType.Valid).Negative().BuildMessage()
            )
        );

    private static Result<CreatePaymentResult> Failure(ErrorDetails error) =>
        Result<CreatePaymentResult>.Failure(error);
}
