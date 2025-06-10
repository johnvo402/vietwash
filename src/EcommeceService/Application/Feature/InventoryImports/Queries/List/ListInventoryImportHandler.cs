using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Suppliers.Query.List;
using Domain.Aggregates.Suppliers.Specifications;
using Domain.Aggregates.Suppliers;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Inventories;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.QueryStringProcessing;
using Domain.Aggregates.Inventories.Specifications;

namespace Application.Feature.InventoryImports.Queries.List
{
    public class ListInventoryImportHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListInventoryImportQuery, PaginationResponse<ListInventoryImportResponse>>
    {
        public async ValueTask<PaginationResponse<ListInventoryImportResponse>> Handle(
            ListInventoryImportQuery query,
            CancellationToken cancellationToken
        )
        {
            try
            {
                return await unitOfWork.Repository<InventoryDocument>().
               PagedListAsync<ListInventoryImportResponse>(
            new ListInventoryImportSpecification(),
            query.ValidateQuery().ValidateFilter(typeof(ListSupplierResponse))

        );
            }
            catch (Exception ex)
            {
                throw new Exception("Exception", ex);
            }
        }


    }
}
