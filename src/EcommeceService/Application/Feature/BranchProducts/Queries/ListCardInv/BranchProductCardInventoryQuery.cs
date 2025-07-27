using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.BranchProducts.Queries.ListCardInv
{
    public class BranchProductCardInventoryQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<BranchProductCardInventoryResponse>>>;
}
