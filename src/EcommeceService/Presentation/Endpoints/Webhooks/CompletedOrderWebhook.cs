using Application.Common.Auth;
using Application.Feature.Orders.Command.UpdateStatus;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Webhooks
{
    public class CompletedOrderWebhook(PayOS payOS, ISender sender)
        : EndpointBaseAsync.WithRequest<WebhookType>.WithActionResult<ApiResponse>
    {
        [HttpPost(Router.Webhook.CompletedOrder)]
        [SwaggerOperation(Tags = [Router.Webhook.Tags], Summary = "Update Status Order")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            WebhookType request,
            CancellationToken cancellationToken = default
        )
        {
            WebhookData data;
            try
            {
                data = payOS.verifyPaymentWebhookData(request);
            }
            catch
            {
                return BadRequest();
            }

            if (!request.success || request.code != "00" || data.code != "00" || data.amount <= 0)
                return BadRequest();

            if (
                data.orderCode == 123
                && data.amount == 3000
                && data.description == "VQRIO123"
                && data.reference == "TF230204212323"
            )
                return Ok();

            UpdateStatusCommand requestSend = UpdateStatusCommand.FromVerifiedPayOsWebhook(
                data.orderCode,
                data.amount,
                new()
                {
                    Status = OrderStatus.Completed,
                    PaymentMethod = PaymentMethod.Card,
                }
            );
            var response = await sender.Send(requestSend, cancellationToken);
            return response.ToActionResult();
        }
    }
}
