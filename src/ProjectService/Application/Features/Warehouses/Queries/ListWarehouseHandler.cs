using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Warehouses;
using Domain.Aggregates.Warehouses.Specifications;
using Mediator;

namespace Application.Features.Warehouses.Queries
{
    public class ListWarehouseHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ListWarehouseQuery, Result<PaginationResponse<ListWarehouseResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<ListWarehouseResponse>>> Handle(
            ListWarehouseQuery request,
            CancellationToken cancellationToken
        )
        {
            var validation = request.Validate<ListWarehouseQuery, ListWarehouseResponse>();

            if (validation != null)
            {
                return validation;
            }
            var response = await unitOfWork
                .DynamicReadOnlyRepository<Warehouse>()
                .PagedListAsync(
                    new ListWarehouseSpecification(),
                    request,
                    WarehouseMapping.Selector(),
                    cancellationToken
                );
            return Result<PaginationResponse<ListWarehouseResponse>>.Success(response);
        }
    }
}
