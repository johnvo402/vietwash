using Application.Feature.Common.Projections.Vouchers;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Vouchers.Commands.Create
{
    public class CreateVoucherCommand : VoucherModel, IRequest<Result>;

}
