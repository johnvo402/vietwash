using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Reports.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Domain.Functions;
using Mediator;

namespace Application.Feature.Reports.CustomerRevenueReport;

public class CustomerRevenueReportHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
    : IRequestHandler<
        CustomerRevenueReportQuery,
        Result<PaginationResponse<CustomerRevenueResult>>
    >
{
    private const string FunctionName = "get_customer_revenue_report";

    public async ValueTask<Result<PaginationResponse<CustomerRevenueResult>>> Handle(
        CustomerRevenueReportQuery request,
        CancellationToken cancellationToken
    )
    {
        ReportBranchScopeResult branchScope = ReportBranchScope.Resolve(
            currentUser.Session?.Branches,
            request.BranchIds
        );
        if (branchScope.HasUnauthorizedBranch)
            return Result<PaginationResponse<CustomerRevenueResult>>.Failure(
                new ForbiddenError(Message.FORBIDDEN)
            );

        ReportUtcRange range = ReportTimeRange.ForUnixSeconds(request.From, request.To);
        object[] parameters =
        [
            branchScope.BranchIds.ToArray(),
            range.UtcStartInclusive,
            range.UtcEndExclusive,
            string.IsNullOrWhiteSpace(request.SearchKeywords)
                ? DBNull.Value
                : request.SearchKeywords,
        ];

        PaginationResponse<CustomerRevenueResult> response = await unitOfWork
            .RepositoryFunction<CustomerRevenueResult>()
            .PagedListAsync(
                functionName: FunctionName,
                parameters: parameters,
                defaultSort: $"{nameof(CustomerRevenueResult.CustomerId)}{OrderTerm.DELIMITER}{OrderTerm.DESC}",
                queryParam: request,
                cancellationToken: cancellationToken
            );
        return Result<PaginationResponse<CustomerRevenueResult>>.Success(response);
    }
}
