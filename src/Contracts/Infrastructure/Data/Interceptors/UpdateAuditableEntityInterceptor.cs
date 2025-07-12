using Application.Common.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Kernel.Common;

namespace Infrastructure.Data.Interceptors;

public class UpdateAuditableEntityInterceptor(ICurrentAccount currentUser) : SaveChangesInterceptor
{
    private const string ANONYMOUS_CREATED_BY = "SYSTEM";

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;

        var entities = context.ChangeTracker.Entries().ToList();

        foreach (var entry in entities)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    SetAuditOnCreate(entry, currentTime);
                    break;

                case EntityState.Modified:
                    SetAuditOnUpdate(entry, currentTime);
                    break;
            }
        }
    }

    private void SetAuditOnCreate(EntityEntry entry, DateTimeOffset currentTime)
    {
        if (
            entry.Entity is BaseEntity
            || entry.Entity is AggregateRoot
            || entry.Entity is IBaseAuditable
        )
        {
            entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue =
                currentUser.Id?.ToString() ?? ANONYMOUS_CREATED_BY;
            return;
        }

        entry.Property(nameof(DefaultEntity.CreatedAt)).CurrentValue = currentTime;
    }

    private void SetAuditOnUpdate(EntityEntry entry, DateTimeOffset currentTime)
    {
        if (entry.Entity is not IAuditable)
        {
            return;
        }

        if (entry.Metadata.FindProperty(nameof(IAuditable.UpdatedBy)) != null)
        {
            entry.Property(nameof(IAuditable.UpdatedBy)).CurrentValue =
                currentUser.Id?.ToString() ?? ANONYMOUS_CREATED_BY;
        }

        if (entry.Metadata.FindProperty(nameof(IAuditable.UpdatedAt)) != null)
        {
            entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = currentTime;
        }
    }
}
