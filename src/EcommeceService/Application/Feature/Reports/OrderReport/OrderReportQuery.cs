using Application.Feature.Common.Projections.Reports;
using Domain.Functions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Reports.OrderReport
{
    public class OrderReportQuery : ReportFilter, IRequest<PaginationResponse<OrderSummaryResult>>;
}
