using Application.Feature.Services.Queries.List;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Suppliers.Query.List
{
    public class ListSupplierQuery : QueryParamRequest, IRequest<PaginationResponse<ListSupplierResponse>>;
}
