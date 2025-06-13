using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Reports.OrderReport;

public class OrderReportHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<OrderReportQuery, PaginationResponse<OrderReportResponse>>
{
    private const string functionName = "get_order_summary";

    public async ValueTask<PaginationResponse<OrderReportResponse>> Handle(
        OrderReportQuery request,
        CancellationToken cancellationToken
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            ["branch_ids"] = request.BranchIds ?? [],
            ["from_time"] = DateTimeOffset
                .FromUnixTimeSeconds(request.From)
                .ToOffset(TimeSpan.FromHours(7)),
            ["to_time"] = DateTimeOffset
                .FromUnixTimeSeconds(request.To)
                .ToOffset(TimeSpan.FromHours(7)),
            ["search"] = string.IsNullOrWhiteSpace(request.SearchKeywords)
                ? DBNull.Value
                : request.SearchKeywords,
        };

        return await unitOfWork
            .RepositoryFunction<OrderReportResponse>()
            .ExecuteFunctionWithPagingAsync(
                functionName: functionName,
                parameters: parameters,
                sort: request.Sort,
                page: request.Page,
                pageSize: request.PageSize,
                defaultSort: "order_date DESC",
                cancellationToken
            );
    }
}
