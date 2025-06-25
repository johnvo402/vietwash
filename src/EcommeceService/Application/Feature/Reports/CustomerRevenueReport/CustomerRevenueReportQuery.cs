using Application.Feature.Common.Projections.Reports;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Domain.Functions;
using Mediator;

namespace Application.Feature.Reports.CustomerRevenueReport
{
    public class CustomerRevenueReportQuery
        : ReportFilter,
            IRequest<Result<PaginationResponse<CustomerRevenueResult>>>;
}
