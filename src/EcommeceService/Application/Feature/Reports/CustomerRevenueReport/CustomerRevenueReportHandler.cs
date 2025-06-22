using Application.Common.Interfaces.UnitOfWorks;
using Domain.Functions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Models;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Reports.CustomerRevenueReport
{
    public class CustomerRevenueReportHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CustomerRevenueReportQuery, PaginationResponse<CustomerRevenueResult>>
    {
        private const string functionName = "get_customer_revenue_report";

        public async ValueTask<PaginationResponse<CustomerRevenueResult>> Handle(
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
            return await unitOfWork
                .RepositoryFunction<CustomerRevenueResult>()
                .PagedListFunctionAsync(
                    functionName: functionName,
                    parameters: parameters,
                    defaultSort: $"{nameof(CustomerRevenueResult.CustomerId)}{OrderTerm.DELIMITER}{OrderTerm.DESC}",
                    queryParam: request,
                    cancellationToken: cancellationToken
                );
        }
    }
}
