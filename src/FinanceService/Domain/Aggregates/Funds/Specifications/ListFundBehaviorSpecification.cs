using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Funds.Specifications
{
    public class ListFundBehaviorSpecification : Specification<FundBehavior>
    {
        public ListFundBehaviorSpecification()
        {

            Query

                .AsNoTracking()
                .AsSplitQuery();

        }
    }
}
