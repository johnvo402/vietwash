using Application.Feature.Orders.Command.UpdateStatus;
using Application.Feature.Orders.Queries.GetLinkPayment;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Webhooks;

public class CompletedOrderWebhook(IOrderPaymentWebhookVerifier verifier, ISender sender)
    : EndpointBaseAsync.WithRequest<WebhookType>.WithActionResult<ApiResponse>
{
    [HttpPost(Router.Webhook.CompletedOrder)]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [Router.Webhook.Tags], Summary = "Complete a PayOS order")]
    public override async Task<ActionResult<ApiResponse>> HandleAsync(
        WebhookType request,
        CancellationToken cancellationToken = default
    )
    {
        WebhookData data;
        try
        {
            data = verifier.Verify(request);
        }
        catch
        {
            return BadRequest();
        }

        if (!PayOsWebhookPolicy.IsSuccessful(request.success, request.code, data.code, data.amount))
            return BadRequest();

        if (
            PayOsWebhookPolicy.IsConfirmationSample(
                data.orderCode,
                data.amount,
                data.description,
                data.reference
            )
        )
            return Ok();

        UpdateStatusCommand command = UpdateStatusCommand.FromVerifiedPayOsWebhook(
            data.orderCode,
            data.amount,
            new() { Status = OrderStatus.Completed, PaymentMethod = PaymentMethod.Card }
        );
        Result response = await sender.Send(command, cancellationToken);
        return response.ToActionResult();
    }
}

public static class PayOsWebhookPolicy
{
    public static bool IsSuccessful(
        bool requestSuccess,
        string? requestCode,
        string? dataCode,
        int amount
    ) => requestSuccess && requestCode == "00" && dataCode == "00" && amount > 0;

    public static bool IsConfirmationSample(
        long orderCode,
        int amount,
        string? description,
        string? reference
    ) =>
        orderCode == 123
        && amount == 3000
        && description == "VQRIO123"
        && reference == "TF230204212323";
}
