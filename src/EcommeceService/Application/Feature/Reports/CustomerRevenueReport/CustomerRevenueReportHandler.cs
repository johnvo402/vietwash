using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Domain.Functions;
using Mediator;

namespace Application.Feature.Reports.CustomerRevenueReport
{
    public class CustomerRevenueReportHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<
            CustomerRevenueReportQuery,
            Result<PaginationResponse<CustomerRevenueResult>>
        >
    {
        private const string functionName = "get_customer_revenue_report";

        public async ValueTask<Result<PaginationResponse<CustomerRevenueResult>>> Handle(
            CustomerRevenueReportQuery request,
            CancellationToken cancellationToken
        )
        {
            // Chuyển đổi parameters từ Dictionary thành mảng object[]
            var parameters = new object[]
            {
                request.BranchIds ?? new List<long>(),
                DateTimeOffset.FromUnixTimeSeconds(request.From).ToOffset(TimeSpan.FromHours(0)),
                DateTimeOffset.FromUnixTimeSeconds(request.To).ToOffset(TimeSpan.FromHours(0)),
                string.IsNullOrWhiteSpace(request.SearchKeywords)
                    ? DBNull.Value
                    : request.SearchKeywords,
            };
            var response = await unitOfWork
                .RepositoryFunction<CustomerRevenueResult>()
                .PagedListAsync(
                    functionName: functionName,
                    parameters: parameters,
                    defaultSort: $"{nameof(CustomerRevenueResult.CustomerId)}{OrderTerm.DELIMITER}{OrderTerm.DESC}",
                    queryParam: request,
                    cancellationToken: cancellationToken
                );
            return Result<PaginationResponse<CustomerRevenueResult>>.Success(response);
        }
    }
}
