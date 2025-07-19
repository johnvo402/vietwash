using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Equipments;
using Specification;
using Specification.Builders;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Domain.Aggregates.Vouchers.Specifications
{
    public class ListVoucherSpecification : Specification<Voucher>
    {
        public ListVoucherSpecification(long? customerId)
        {
            if (customerId.HasValue)
            {
                Query.Where(v =>
                    v.VoucherCustomers.Any(vc =>
                        vc.CustomerId == customerId.Value && vc.IsUsed == false

                    ) && v.Status == 0
                );
            }

            Query.Include(v => v.VoucherCustomers).AsNoTracking().AsSplitQuery();
        }
    }
}
