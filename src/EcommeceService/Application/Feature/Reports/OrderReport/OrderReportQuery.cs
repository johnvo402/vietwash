using Application.Feature.Common.Projections.Reports;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Domain.Functions;
using Mediator;

namespace Application.Feature.Reports.OrderReport
{
    public class OrderReportQuery : ReportFilter, IRequest<Result<PaginationResponse<OrderSummaryResult>>>;
}
