using Application.Feature.Common.Projections.Reports;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Reports.FinancialReport
{
    public class FinancialReportQuery : ReportFilter, IRequest<Result<FinancialReportResponse>>;
}
