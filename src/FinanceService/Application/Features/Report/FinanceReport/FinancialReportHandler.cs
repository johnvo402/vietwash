using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

using Application.Common.Interfaces.Services;
using Application.Features.Report.Common;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;

namespace Application.Features.Report.FinanceReport
{
    public class FinancialReportHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
        : IRequestHandler<FinancialReportQuery, Result<FinancialReportResponse>>
    {
        public async ValueTask<Result<FinancialReportResponse>> Handle(
            FinancialReportQuery request,
            CancellationToken cancellationToken
        )
        {
            ReportBranchScopeResult branchScope = ReportBranchScope.Resolve(
                currentUser.Session?.Branches,
                request.BranchIds
            );
            if (branchScope.HasUnauthorizedBranch)
                return Result<FinancialReportResponse>.Failure(
                    new ForbiddenError(Message.FORBIDDEN)
                );

            var from = DateTimeOffset
                .FromUnixTimeSeconds(request.From)
                .ToOffset(TimeSpan.FromHours(7));
            var to = DateTimeOffset.FromUnixTimeSeconds(request.To).ToOffset(TimeSpan.FromHours(0));

            var groupedAmounts = await unitOfWork
                .Repository<Fund>()
                .QueryAsync()
                .Where(f =>
                    f.CreatedAt >= from
                    && f.CreatedAt <= to
                    && f.Status == FundStatus.Confirmed
                    && branchScope.BranchIds.Contains(f.BranchId)
                )
                .GroupBy(f => f.FundBehaviorId)
                .Select(g => new { FundBehaviorId = g.Key, TotalAmount = g.Sum(f => f.Amount) })
                .ToListAsync(cancellationToken);

            var report = new FinancialReportResponse
            {
                TotalOtherIncome =
                    groupedAmounts.FirstOrDefault(x => x.FundBehaviorId == 3)?.TotalAmount ?? 0,
                TotalOtherSpend =
                    groupedAmounts.FirstOrDefault(x => x.FundBehaviorId == 4)?.TotalAmount ?? 0,
                TotalStockExport =
                    groupedAmounts.FirstOrDefault(x => x.FundBehaviorId == 5)?.TotalAmount ?? 0,
                TotalStockImport =
                    groupedAmounts.FirstOrDefault(x => x.FundBehaviorId == 6)?.TotalAmount ?? 0,
            };

            return Result<FinancialReportResponse>.Success(report);
        }
    }
}
