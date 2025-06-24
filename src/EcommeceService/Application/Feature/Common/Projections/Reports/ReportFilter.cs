using Contracts.Dtos.Requests;

namespace Application.Feature.Common.Projections.Reports;

public class ReportFilter : QueryParamRequest
{
    public long From { get; set; }
    public long To { get; set; }
    public List<long>? BranchIds { get; set; }
    public string? SearchKeywords { get; set; }
}
