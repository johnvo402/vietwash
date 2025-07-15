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
        [HttpPost(Router.Webhooks.CompletedOrder)]
        [SwaggerOperation(Tags = [Router.Webhooks.Tags], Summary = "Update Status Order")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            WebhookType request,
            CancellationToken cancellationToken = default
        )
        {
            WebhookData data = payOS.verifyPaymentWebhookData(request);
            if (data.code == "00")
            {
                if (data.orderCode == 123)
                {
                    return Ok();
                }
                var requestSend = new UpdateStatusCommand
                {
                    OrderId = data.orderCode.ToString(),
                    Status = OrderStatus.Completed,
                    PaymentMethod = PaymentMethod.Card,
                };
                var response = await sender.Send(requestSend, cancellationToken);
                return response.ToActionResult();
            }
            return BadRequest();
        }
    }
}
