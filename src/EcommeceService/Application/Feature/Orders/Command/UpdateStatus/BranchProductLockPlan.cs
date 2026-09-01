namespace Application.Feature.Orders.Command.UpdateStatus;

public sealed record BranchProductLockPlan(
    string Sql,
    IReadOnlyList<long> BranchProductIds,
    object[] Parameters
)
{
    public static BranchProductLockPlan Create(IEnumerable<long> branchProductIds)
    {
        long[] orderedIds = branchProductIds.Distinct().OrderBy(x => x).ToArray();
        if (orderedIds.Length == 0 || orderedIds.Any(x => x <= 0))
            throw new ArgumentException(
                "At least one valid BranchProduct identifier is required.",
                nameof(branchProductIds)
            );

        string placeholders = string.Join(", ", orderedIds.Select((_, index) => $"{{{index}}}"));
        string sql =
            $"SELECT id FROM branch_product WHERE id IN ({placeholders}) ORDER BY id FOR UPDATE";

        return new BranchProductLockPlan(sql, orderedIds, orderedIds.Cast<object>().ToArray());
    }
}
