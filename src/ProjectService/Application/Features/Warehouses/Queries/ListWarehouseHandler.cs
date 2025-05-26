using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Common.QueryStringProcessing;
using Domain.Aggregates.Warehouses;
using Domain.Aggregates.Warehouses.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Features.Warehouses.Queries
{
    public class ListWarehouseHandler(IUnitOfWork unitOfWork) : IRequestHandler<ListWarehouseQuery, PaginationResponse<ListWarehouseResponse>>
    {
        public async ValueTask<PaginationResponse<ListWarehouseResponse>> Handle(ListWarehouseQuery request, CancellationToken cancellationToken)
        => await unitOfWork.CachedRepository<Warehouse>()
                            .PagedListAsync<ListWarehouseResponse>
                                (
                                new ListWarehouseSpecification(),
                                request.ValidateQuery().ValidateFilter(typeof(ListWarehouseResponse))
                                );
    }
}
