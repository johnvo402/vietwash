using Application.Feature.Common.Projections.Reports;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Reports.Queries.ServiceOrderReport
{
    public class ServiceRevenueReportQuery
        : ReportFilter,
            IRequest<Result<PaginationResponse<ServiceRevenueReportResponse>>>;
}
