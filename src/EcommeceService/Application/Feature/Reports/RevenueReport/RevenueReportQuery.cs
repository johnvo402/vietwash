using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Feature.Common.Projections.Reports;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Reports.RevenueReport
{
    public class RevenueReportQuery
        : ReportFilter,
            IRequest<Result<PaginationResponse<RevenueReportResponse>>>;
}
