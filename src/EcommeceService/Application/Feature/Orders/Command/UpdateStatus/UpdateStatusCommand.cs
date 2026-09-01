using Application.Feature.Common.Projections.Orders;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Application.Feature.Orders.Command.UpdateStatus
{
    public class UpdateStatusCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public string OrderId { get; set; } = string.Empty;

        [FromBody]
        public OrderUpdateStatus Model { get; set; } = default!;

        [JsonIgnore]
        public int? ExpectedPaymentAmount { get; private init; }

        [JsonIgnore]
        internal bool IsVerifiedPayOsWebhook { get; private init; }

        public static UpdateStatusCommand FromVerifiedPayOsWebhook(
            long orderId,
            int expectedPaymentAmount,
            OrderUpdateStatus model
        ) =>
            new()
            {
                OrderId = orderId.ToString(),
                ExpectedPaymentAmount = expectedPaymentAmount,
                IsVerifiedPayOsWebhook = true,
                Model = model,
            };
    }
}
