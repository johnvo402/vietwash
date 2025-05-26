using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Branches.Specifications
{
    public class ListBranchSpecification : Specification<Domain.Aggregates.Branches.Branch>
    {
        public ListBranchSpecification()
        {
            Query.AsNoTracking().AsSplitQuery();
            string key = GetUniqueCachedKey();
            Query.EnableCache(key);
        }
    }
}
