namespace Application.Feature.Common.Projections.Reports;

public class ReportFilter
{
    public long From { get; set; }
    public long To { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public string? Sort { get; set; }
    public List<long>? BranchIds { get; set; }
    public string? SearchKeywords { get; set; }
}
