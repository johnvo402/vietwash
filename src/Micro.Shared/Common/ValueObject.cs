namespace Micro.Shared.Common;

public abstract record ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }
}
