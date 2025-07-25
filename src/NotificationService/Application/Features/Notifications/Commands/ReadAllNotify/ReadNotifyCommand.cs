using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Notifications.Commands.ReadAllNotify
{
    public class ReadAllNotifyCommand : IRequest<Result>;
}
