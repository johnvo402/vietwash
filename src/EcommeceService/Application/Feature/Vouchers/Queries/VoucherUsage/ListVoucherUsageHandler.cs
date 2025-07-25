using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Vouchers.Queries.List;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Specifications;
using Infrastructure.Constants;
using Mediator;

namespace Application.Feature.Vouchers.Queries.VoucherUsage
{
    public class ListVoucherUsageHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
        : IRequestHandler<ListVoucherUsageQuery, Result<PaginationResponse<ListVoucherUsageResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<ListVoucherUsageResponse>>> Handle(
            ListVoucherUsageQuery query,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var validation = query.Validate<ListVoucherUsageQuery, ListVoucherUsageResponse>();

                if (validation != null)
                {
                    return validation;
                }
                long? customerId = null;
                if (currentUser.Session!.Role == ROLE.CUSTOMER)
                {
                    customerId = currentUser.Id;
                }
                var response = await unitOfWork
                    .DynamicReadOnlyRepository<Domain.Aggregates.Vouchers.VoucherUsage>()
                    .PagedListAsync(
                        new ListVoucherUsageSpecification(customerId),
                        query,
                        ListVoucherUsageMapping.Selector(),
                        cancellationToken
                    );

                return Result<PaginationResponse<ListVoucherUsageResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception", ex);
            }
        }
    }
}
