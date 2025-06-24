using Application.Feature.Common.Projections.Services;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Services.Command.Create
{
    public class CreateServiceCommand : ServiceModel, IRequest<Result>;
}
