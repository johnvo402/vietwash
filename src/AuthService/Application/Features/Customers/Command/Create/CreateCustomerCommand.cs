using Application.Features.Common.Projections.Customers;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Customers.Command.Create
{
    public class CreateCustomerCommand : CustomerModel, IRequest<Result<CreateCustomerResponse>>;
}
