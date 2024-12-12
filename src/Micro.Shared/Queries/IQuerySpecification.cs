namespace Micro.Shared.Queries;

public interface IQuerySpecification<T>
{
    IQueryable<T> Apply(IQueryable<T> query);
}