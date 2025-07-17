using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Notifications.Commands.ReadNotify
{
    public class ReadNotifyCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long Id { get; set; }
    }
}
