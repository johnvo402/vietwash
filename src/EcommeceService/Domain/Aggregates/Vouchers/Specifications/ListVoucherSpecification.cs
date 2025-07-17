using Domain.Aggregates.Equipments;
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
    public class ListVoucherSpecification : Specification<Voucher>
    {
        public ListVoucherSpecification()
        {
            Query
                .AsNoTracking()
                .AsSplitQuery();
            string key = GetUniqueCachedKey();
            Query.EnableCache(key);
        }
    }
}
