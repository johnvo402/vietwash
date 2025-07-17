using Specification;
using Specification.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Domain.Aggregates.Vouchers.Specifications
{
    public class GetUsageVoucherSpecification : Specification<VoucherUsage>
    {
        public GetUsageVoucherSpecification(long? voucherId, long? customerId)
        {
            if (voucherId.HasValue)
            {
                Query.Where(x => x.VoucherId == voucherId.Value);
            }

            if (customerId.HasValue)
            {
                Query.Where(x => x.CustomerId == customerId.Value);
            }
        }
    }
}
