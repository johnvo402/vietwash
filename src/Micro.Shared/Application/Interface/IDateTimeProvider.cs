namespace Micro.Shared.Application.Interface;
public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }
}