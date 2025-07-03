using System.ComponentModel.DataAnnotations.Schema;
using Shared.Kernel.Exceptions;

namespace Shared.Kernel.Common;

public abstract class DefaultEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; } = IdGenerator.NewId();
    public Ulid PublicId { get; set; } = Ulid.NewUlid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public abstract class DefaultEntity<T>
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public T Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    protected DefaultEntity()
    {
        if (typeof(T) == typeof(long))
        {
            Id = (T)(object)IdGenerator.NewId();
        }
        else
        {
            Id = default!;
        }
    }
}

public abstract class BaseEntity : DefaultEntity, IAuditable
{
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public abstract class BaseEntity<T> : DefaultEntity<T>, IAuditable
{
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public interface IAuditable : IBaseAuditable;
