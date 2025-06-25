namespace Shared.Kernel.Common;

/// <summary>
/// Cung cấp các phương thức tiện ích để kiểm tra tính hợp lệ của các kiểu liên quan đến BaseEntity.
/// </summary>
public static class FilterBaseEntity
{
    /// <summary>
    /// Kiểm tra xem kiểu có kế thừa từ BaseEntity, AggregateRoot hoặc BaseEntity&lt;T&gt; hay không.
    /// </summary>
    /// <param name="type">Kiểu cần kiểm tra.</param>
    /// <exception cref="ArgumentNullException">Ném ra khi <paramref name="type"/> là null.</exception>
    /// <exception cref="ArgumentException">Ném ra khi kiểu không kế thừa từ BaseEntity, AggregateRoot hoặc BaseEntity&lt;T&gt;.</exception>
    public static void IsValidBaseType(this Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (!Validate(type))
        {
            throw new ArgumentException(
                $"Kiểu {type.FullName} không kế thừa từ BaseEntity, AggregateRoot hoặc BaseEntity<T>.",
                nameof(type)
            );
        }
    }

    /// <summary>
    /// Xác thực xem kiểu hoặc bất kỳ kiểu cha nào của nó có phải là BaseEntity, AggregateRoot hoặc BaseEntity&lt;T&gt;.
    /// </summary>
    /// <param name="type">Kiểu cần xác thực.</param>
    /// <returns>True nếu kiểu hợp lệ, ngược lại là false.</returns>
    private static bool Validate(Type type)
    {
        Type? currentBaseType = type;
        while (currentBaseType != null)
        {
            if (
                currentBaseType == typeof(BaseEntity)
                || currentBaseType == typeof(AggregateRoot)
                || (
                    currentBaseType.IsGenericType
                    && currentBaseType.GetGenericTypeDefinition() == typeof(BaseEntity<>)
                )
            )
            {
                return true;
            }
            currentBaseType = currentBaseType.BaseType;
        }
        return false;
    }
}
