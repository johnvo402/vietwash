namespace Application.Feature.Reports.Common;

public static class ReportBranchScope
{
    public static ReportBranchScopeResult Resolve(
        IEnumerable<string>? authorizedBranchIds,
        IEnumerable<long>? requestedBranchIds
    )
    {
        HashSet<long> authorized = (authorizedBranchIds ?? [])
            .Select(value => long.TryParse(value, out long branchId) ? branchId : (long?)null)
            .Where(branchId => branchId.HasValue)
            .Select(branchId => branchId!.Value)
            .ToHashSet();

        long[] requested = (requestedBranchIds ?? []).Distinct().ToArray();
        if (requested.Length == 0)
            return new ReportBranchScopeResult(authorized.Order().ToArray(), false);

        bool hasUnauthorizedBranch = requested.Any(branchId => !authorized.Contains(branchId));
        return new ReportBranchScopeResult(requested, hasUnauthorizedBranch);
    }

    public static bool IsAuthorized(IEnumerable<string>? authorizedBranchIds, long branchId) =>
        Resolve(authorizedBranchIds, [branchId]) is { HasUnauthorizedBranch: false };
}

public sealed record ReportBranchScopeResult(
    IReadOnlyList<long> BranchIds,
    bool HasUnauthorizedBranch
);
