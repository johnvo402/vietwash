using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Vouchers.Specifications;
using Domain.Aggregates.Vouchers;
using Mediator;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;

namespace Application.Feature.Vouchers.Queries.List
{
    public class ListVoucherHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListVoucherQuery, Result<PaginationResponse<ListVoucherResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<ListVoucherResponse>>> Handle(
            ListVoucherQuery query,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var validation = query.Validate<ListVoucherQuery, ListVoucherResponse>();

                if (validation != null)
                {
                    return validation;
                }

                var response = await unitOfWork
                    .DynamicReadOnlyRepository<Voucher>()
                    .PagedListAsync(
                        new ListVoucherSpecification(),
                        query,
                        ListVoucherMapping.Selector(),
                        cancellationToken
                    );

                return Result<PaginationResponse<ListVoucherResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception", ex);
            }
        }
    }

}
