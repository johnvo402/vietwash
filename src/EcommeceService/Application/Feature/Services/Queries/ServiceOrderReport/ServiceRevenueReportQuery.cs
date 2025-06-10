

using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Feature.Services.Queries.ServiceOrderReport
{
    public class ServiceRevenueReportQuery : IRequest<List<ServiceRevenueReportResponse>>
    {
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}
