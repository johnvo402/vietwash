namespace Application.Feature.Orders.Common;

public sealed class OrderBranchAccess
{
    private readonly HashSet<long> authorizedBranchIds;

    private OrderBranchAccess(HashSet<long> authorizedBranchIds)
    {
        this.authorizedBranchIds = authorizedBranchIds;
        BranchIds = authorizedBranchIds.Order().ToArray();
    }

    public IReadOnlyList<long> BranchIds { get; }

    public static OrderBranchAccess FromSession(IEnumerable<string>? sessionBranches)
    {
        HashSet<long> branchIds = [];
        foreach (string value in sessionBranches ?? [])
        {
            if (long.TryParse(value, out long branchId) && branchId > 0)
                branchIds.Add(branchId);
        }

        return new OrderBranchAccess(branchIds);
    }

    public bool IsAuthorized(long branchId) =>
        branchId > 0 && authorizedBranchIds.Contains(branchId);
}

public sealed record OrderBranchReference(long BranchId);
