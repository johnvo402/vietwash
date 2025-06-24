using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Suppliers;
using Domain.Aggregates.Suppliers.Specifications;
using Mediator;

namespace Application.Feature.Suppliers.Query.List
{
    public class ListSupplierHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ListSupplierQuery, Result<PaginationResponse<ListSupplierResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<ListSupplierResponse>>> Handle(
            ListSupplierQuery query,
            CancellationToken cancellationToken
        )
        {
            var validation = query.Validate<ListSupplierQuery, ListSupplierResponse>();

            if (validation != null)
            {
                return validation;
            }

            var response = await unitOfWork
                .DynamicReadOnlyRepository<Supplier>()
                .CursorPagedListAsync(
                    new ListSupplierSpecification(),
                    query,
                    ListSupplierMapping.Selector(),
                    cancellationToken: cancellationToken
                );
            return Result<PaginationResponse<ListSupplierResponse>>.Success(response);
        }
    }
}
