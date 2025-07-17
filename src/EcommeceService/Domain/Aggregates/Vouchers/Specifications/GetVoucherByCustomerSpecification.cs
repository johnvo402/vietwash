using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Vouchers;
using Microsoft.EntityFrameworkCore;
using Specification;
using Specification.Builders;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class GetVoucherByCustomerSpecification : Specification<Voucher>
{
    public GetVoucherByCustomerSpecification(long voucherId, short customerGroupValue)
    {
        Query.Where(x =>
            x.Id == voucherId &&
            x.CustomerGroups.Contains((CustomerGroup)customerGroupValue)
        );
    }

}

