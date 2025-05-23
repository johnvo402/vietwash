using Application.Features.Common.Projections.Funds;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;


namespace Application.Features.Funds.Command.Update
{
    public class UpdateFundCommand : IRequest
    {
        [FromRoute(Name = RouterBase.Id)]
        public string FundId { get; set; } = string.Empty;

        public UpdateFundModel? updateFundModel { get; set; }   

    }
}
