using Micro.Shared.Queries;

namespace Micro.Shared.Queries;

public class PaginationSpecification<T> : IQuerySpecification<T>
{
    private readonly int? _offset;
    private readonly int? _limit;

    public PaginationSpecification(int? offset, int? limit)
    {
        _offset = offset;
        _limit = limit;
    }

    public IQueryable<T> Apply(IQueryable<T> query)
    {
        if (_offset.HasValue)
        {
            query = query.Skip(_offset.Value);
        }

        if (_limit.HasValue)
        {
            query = query.Take(_limit.Value);
        }

        return query;
    }
}
