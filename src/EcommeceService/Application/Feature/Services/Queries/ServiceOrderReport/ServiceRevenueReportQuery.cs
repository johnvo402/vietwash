using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Services.Queries.ServiceOrderReport
{
    public class ServiceRevenueReportQuery : IRequest<Result<List<ServiceRevenueReportResponse>>>
    {
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}
