using Application.Feature.Orders.Queries.GetLinkPayment;
using Net.payOS.Errors;
using Net.payOS.Types;
using Serilog;

namespace Application.Feature.Orders.Command.UpdateStatus;

public static class ProcessedOrderPaymentCancellation
{
    public static async Task<ProcessedOrderPaymentCancellationResult> EnsureSafeAsync(
        IOrderPaymentLinkClient paymentClient,
        long orderId,
        string cancellationReason,
        ILogger logger
    )
    {
        PaymentLinkInformation paymentLink;
        try
        {
            paymentLink = await paymentClient.GetPaymentLinkInformationAsync(orderId);
        }
        catch (PayOSError error) when (PayOsErrorPolicy.IsPaymentLinkNotFound(error))
        {
            return ProcessedOrderPaymentCancellationResult.Safe(
                ProcessedOrderPaymentState.NotFound
            );
        }
        catch (Exception error)
        {
            LogProviderFailure(logger, orderId, "inspect", error);
            return ProcessedOrderPaymentCancellationResult.Rejected(
                "The payment link could not be verified. The order was not cancelled."
            );
        }

        ProcessedOrderPaymentState state = GetState(paymentLink.status);
        switch (state)
        {
            case ProcessedOrderPaymentState.Cancelled:
                return ProcessedOrderPaymentCancellationResult.Safe(state);
            case ProcessedOrderPaymentState.Paid:
                return ProcessedOrderPaymentCancellationResult.Rejected(
                    state,
                    "This order has already been paid and cannot be cancelled."
                );
            case ProcessedOrderPaymentState.Processing:
                return ProcessedOrderPaymentCancellationResult.Rejected(
                    state,
                    "Payment is currently processing. Retry after payment status is resolved."
                );
            case ProcessedOrderPaymentState.Pending:
                return await CancelPendingAsync(paymentClient, orderId, cancellationReason, logger);
            default:
                LogUnknownState(logger, orderId, paymentLink.status);
                return ProcessedOrderPaymentCancellationResult.Rejected(
                    state,
                    "The payment link returned an unknown state. The order was not cancelled."
                );
        }
    }

    private static async Task<ProcessedOrderPaymentCancellationResult> CancelPendingAsync(
        IOrderPaymentLinkClient paymentClient,
        long orderId,
        string cancellationReason,
        ILogger logger
    )
    {
        try
        {
            PaymentLinkInformation cancelled = await paymentClient.CancelPaymentLinkAsync(
                orderId,
                cancellationReason
            );
            ProcessedOrderPaymentState resultState = GetState(cancelled.status);
            if (resultState == ProcessedOrderPaymentState.Cancelled)
                return ProcessedOrderPaymentCancellationResult.Safe(resultState);

            if (resultState == ProcessedOrderPaymentState.Unknown)
                LogUnknownState(logger, orderId, cancelled.status);

            return RejectedForState(resultState);
        }
        catch (Exception cancelError)
        {
            LogProviderFailure(logger, orderId, "cancel", cancelError);

            // A timeout can happen after payOS accepted the cancellation. Recheck once only.
            try
            {
                PaymentLinkInformation rechecked =
                    await paymentClient.GetPaymentLinkInformationAsync(orderId);
                ProcessedOrderPaymentState recheckedState = GetState(rechecked.status);
                if (recheckedState == ProcessedOrderPaymentState.Cancelled)
                    return ProcessedOrderPaymentCancellationResult.Safe(recheckedState);

                if (recheckedState == ProcessedOrderPaymentState.Unknown)
                    LogUnknownState(logger, orderId, rechecked.status);

                return RejectedForState(recheckedState);
            }
            catch (Exception recheckError)
            {
                LogProviderFailure(logger, orderId, "recheck", recheckError);
                return ProcessedOrderPaymentCancellationResult.Rejected(
                    "The payment link cancellation could not be confirmed. The order was not cancelled."
                );
            }
        }
    }

    private static ProcessedOrderPaymentCancellationResult RejectedForState(
        ProcessedOrderPaymentState state
    ) =>
        state switch
        {
            ProcessedOrderPaymentState.Paid => ProcessedOrderPaymentCancellationResult.Rejected(
                state,
                "This order has already been paid and cannot be cancelled."
            ),
            ProcessedOrderPaymentState.Processing =>
                ProcessedOrderPaymentCancellationResult.Rejected(
                    state,
                    "Payment is currently processing. Retry after payment status is resolved."
                ),
            _ => ProcessedOrderPaymentCancellationResult.Rejected(
                state,
                "The payment link cancellation was not confirmed. The order was not cancelled."
            ),
        };

    public static ProcessedOrderPaymentState GetState(string? state) =>
        state?.Trim().ToUpperInvariant() switch
        {
            "PENDING" => ProcessedOrderPaymentState.Pending,
            "PROCESSING" => ProcessedOrderPaymentState.Processing,
            "PAID" => ProcessedOrderPaymentState.Paid,
            "CANCELLED" => ProcessedOrderPaymentState.Cancelled,
            _ => ProcessedOrderPaymentState.Unknown,
        };

    private static void LogUnknownState(ILogger logger, long orderId, string? providerState) =>
        logger.Warning(
            "Cannot cancel Order {OrderId} with payOS order code {PayOsOrderCode}: unknown provider state {ProviderState}",
            orderId,
            orderId,
            providerState
        );

    private static void LogProviderFailure(
        ILogger logger,
        long orderId,
        string operation,
        Exception error
    ) =>
        logger.Warning(
            "Cannot cancel Order {OrderId} with payOS order code {PayOsOrderCode}: provider {Operation} failed with {ProviderErrorType} and code {ProviderErrorCode}",
            orderId,
            orderId,
            operation,
            error.GetType().Name,
            error is PayOSError payOsError ? payOsError.Code : null
        );
}

public static class PayOsErrorPolicy
{
    // payOS 1.0.9 exposes the provider response code but not the HTTP status.
    // Code 231 is the provider's specific "payment request not found" response.
    public const string PaymentLinkNotFoundCode = PayOsOrderPolicy.PaymentLinkNotFoundCode;

    public static bool IsPaymentLinkNotFound(PayOSError error) =>
        string.Equals(error.Code, PaymentLinkNotFoundCode, StringComparison.Ordinal);
}

public sealed record ProcessedOrderPaymentCancellationResult(
    bool IsSafe,
    ProcessedOrderPaymentState State,
    string? ErrorMessage
)
{
    public static ProcessedOrderPaymentCancellationResult Safe(ProcessedOrderPaymentState state) =>
        new(true, state, null);

    public static ProcessedOrderPaymentCancellationResult Rejected(string errorMessage) =>
        new(false, ProcessedOrderPaymentState.Unknown, errorMessage);

    public static ProcessedOrderPaymentCancellationResult Rejected(
        ProcessedOrderPaymentState state,
        string errorMessage
    ) => new(false, state, errorMessage);
}

public enum ProcessedOrderPaymentState
{
    Unknown,
    NotFound,
    Pending,
    Processing,
    Paid,
    Cancelled,
}
