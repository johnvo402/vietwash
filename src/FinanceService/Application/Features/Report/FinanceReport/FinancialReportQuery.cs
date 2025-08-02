

using Application.Features.Common.Projections.Reports;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Report.FinanceReport
{
    public class FinancialReportQuery : ReportFilter,
            IRequest<Result<FinancialReportResponse>>;
}
