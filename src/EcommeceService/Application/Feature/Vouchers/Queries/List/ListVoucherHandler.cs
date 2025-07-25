using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Specifications;
using Infrastructure.Constants;
using Mediator;

namespace Application.Feature.Vouchers.Queries.List
{
    public class ListVoucherHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
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
                long? customerId = null;
                if (currentUser.Session!.Role == ROLE.CUSTOMER)
                {
                    customerId = currentUser.Id;
                }
                var response = await unitOfWork
                    .DynamicReadOnlyRepository<Voucher>()
                    .PagedListAsync(
                        new ListVoucherSpecification(customerId),
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
