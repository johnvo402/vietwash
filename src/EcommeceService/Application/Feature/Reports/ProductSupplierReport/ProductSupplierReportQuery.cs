using Application.Feature.Common.Projections.Reports;
using Application.Feature.Reports.RevenueReport;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Reports.ProductSupplierReport
{
    public class ProductSupplierReportQuery
        : ReportFilter,
            IRequest<Result<PaginationResponse<ProductSupplierReportResponse>>>;
}
