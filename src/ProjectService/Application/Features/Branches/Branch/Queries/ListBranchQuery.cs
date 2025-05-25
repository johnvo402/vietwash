using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Features.Branches.Branch.Queries
{
    public class ListBranchQuery : QueryParamRequest, IRequest<PaginationResponse<ListBranchResponse>>
    {
    }
}
