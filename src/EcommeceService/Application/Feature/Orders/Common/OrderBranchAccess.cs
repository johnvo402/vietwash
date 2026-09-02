using Infrastructure.Constants;

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

public static class OrderActorAccess
{
    public static bool IsCustomer(string? role) => role == ROLE.CUSTOMER;

    public static bool IsStaffSide(string? role) =>
        role is ROLE.ADMIN or ROLE.MANAGER or ROLE.STAFF;

    public static bool CanReadOrder(
        string? role,
        long? currentAccountId,
        IEnumerable<string>? sessionBranches,
        long? orderCustomerId,
        long orderBranchId
    )
    {
        if (IsCustomer(role))
            return currentAccountId is > 0 && orderCustomerId == currentAccountId;

        return IsStaffSide(role)
            && OrderBranchAccess.FromSession(sessionBranches).IsAuthorized(orderBranchId);
    }

    public static bool CanOperateOrder(
        string? role,
        IEnumerable<string>? sessionBranches,
        long orderBranchId
    ) =>
        IsStaffSide(role)
        && OrderBranchAccess.FromSession(sessionBranches).IsAuthorized(orderBranchId);
}

public sealed record OrderBranchReference(long BranchId, long? CustomerId = null);
