using Application.Features.Common.Projections.Funds;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Funds.Command.Update
{
    public class UpdateFundCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public string FundId { get; set; } = string.Empty;

        [FromBody]
        public UpdateFundModel UpdateFundModel { get; set; } = default!;
    }
}
