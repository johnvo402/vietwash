using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Models;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Domain.Functions;
using Mediator;

namespace Application.Feature.Reports.OrderReport;

public class OrderReportHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<OrderReportQuery, Result<PaginationResponse<OrderSummaryResult>>>
{
    private const string functionName = "get_order_summary";

    public async ValueTask<Result<PaginationResponse<OrderSummaryResult>>> Handle(
        OrderReportQuery request,
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
            .RepositoryFunction<OrderSummaryResult>()
            .PagedListAsync(
                functionName: functionName,
                parameters: parameters,
                defaultSort: $"{nameof(OrderSummaryResult.OrderId)}{OrderTerm.DELIMITER}{OrderTerm.DESC}",
                queryParam: request,
                cancellationToken: cancellationToken
            );
        return Result<PaginationResponse<OrderSummaryResult>>.Success(response);
    }
}
