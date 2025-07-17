using Application.Feature.Common.Projections.Equipments;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Vouchers
{
    public class VoucherDetailProjection : VoucherProjection
    {
        public override void MappingFrom(Voucher voucher)
        {
            base.MappingFrom(voucher);

        }
    }
}
