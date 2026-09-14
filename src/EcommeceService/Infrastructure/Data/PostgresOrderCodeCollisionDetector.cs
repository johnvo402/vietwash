using Application.Feature.Orders.Command.Create;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Data;

public sealed class PostgresOrderCodeCollisionDetector : IOrderCodeCollisionDetector
{
    public const string ConstraintName = "ix_order_code";

    public bool IsOrderCodeCollision(Exception exception) =>
        exception is DbUpdateException
        {
            InnerException: PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: ConstraintName,
            },
        };
}
