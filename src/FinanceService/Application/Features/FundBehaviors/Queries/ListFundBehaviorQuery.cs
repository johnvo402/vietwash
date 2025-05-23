using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FundBehaviors.Queries
{
    public class ListFundBehaviorQuery : QueryParamRequest, IRequest<IEnumerable<ListFundBehaviorResponse>>;


}
