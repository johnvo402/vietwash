using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Accounts.Specifications
{
    public class GetAccountContactByAccountIdSpecification : Specification<AccountContact>
    {
        public GetAccountContactByAccountIdSpecification(long accountId)
        {
            Query.Where(x => x.AccountId == accountId)
                 .Include(x => x.Account)
                 .AsSplitQuery();
        }
    }
}
