using System.Linq.Expressions;
using Domain.Aggregates.Services;

namespace Application.Feature.Units.Queries.List
{
    public static class ListUnitMapping
    {
        public static Expression<Func<Unit, ListUnitResponse>> Selector() =>
            unit => new ListUnitResponse
            {
                Id = unit.Id,
                Name = unit.Name,
                Status = unit.Status,
            };
    }
}
