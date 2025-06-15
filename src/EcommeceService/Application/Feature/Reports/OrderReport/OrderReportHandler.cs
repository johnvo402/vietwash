using Application.Common.Interfaces.UnitOfWorks;
using Domain.Functions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Models;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Reports.OrderReport;

public class OrderReportHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<OrderReportQuery, PaginationResponse<OrderSummaryResult>>
{
    private const string functionName = "get_order_summary";

    public async ValueTask<PaginationResponse<OrderSummaryResult>> Handle(
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

        return await unitOfWork
            .RepositoryFunction<OrderSummaryResult>()
            .PagedListFunctionAsync(
                functionName: functionName,
                parameters: parameters,
                defaultSort: $"{nameof(OrderSummaryResult.OrderId)}{OrderTerm.DELIMITER}{OrderTerm.DESC}",
                queryParam: request,
                cancellationToken: cancellationToken
            );
    }
}
