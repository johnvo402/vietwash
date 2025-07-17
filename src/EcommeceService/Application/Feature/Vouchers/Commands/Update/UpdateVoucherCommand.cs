using Application.Feature.Common.Projections.Vouchers;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Vouchers.Commands.Update
{
    public class UpdateVoucherCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long VoucherId { get; set; } = default!;

        [FromBody]
        public VoucherModel Voucher { get; set; } = default!;
    }
}
