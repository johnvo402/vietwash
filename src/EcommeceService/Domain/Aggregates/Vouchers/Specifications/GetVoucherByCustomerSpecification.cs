using System.Linq;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Vouchers;
using Microsoft.EntityFrameworkCore;
using Specification;
using Specification.Builders;

public class GetVoucherByCustomerSpecification : Specification<Voucher>
{
    public GetVoucherByCustomerSpecification(
        long voucherId,
        short customerGroupValue,
        long customerId
    )
    {
        var customerGroup = (CustomerGroup)customerGroupValue;

        Query.Where(x =>
            x.Id == voucherId
            // && (
            //     x.VoucherCustomerGroups.Any(vcg => vcg.Group == customerGroup)
            //     || x.VoucherCustomers.Any(vc => vc.CustomerId == customerId)
            // )
        );
    }
}
