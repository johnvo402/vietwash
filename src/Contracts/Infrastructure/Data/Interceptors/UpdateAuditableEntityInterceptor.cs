using Application.Common.Interfaces.Services;
using Contracts.Application.Common.Interfaces.GenIdLong;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Kernel.Common;

namespace Infrastructure.Data.Interceptors;

public class UpdateAuditableEntityInterceptor(ICurrentAccount currentUser, IIdGenerator idGenerator)
    : SaveChangesInterceptor
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

        var entities = context
            .ChangeTracker.Entries()
            .Where(e =>
                e.Entity is BaseEntity || e.Entity is AggregateRoot || e.Entity is IBaseAuditable
            )
            .ToList();

        foreach (var entry in entities)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    SetIdIfNeeded(entry);
                    SetPublicIdIfNeeded(entry);
                    SetAuditOnCreate(entry, currentTime);
                    break;

                case EntityState.Modified:
                    SetAuditOnUpdate(entry, currentTime);
                    break;
            }
        }
    }

    private void SetIdIfNeeded(EntityEntry entry)
    {
        var idProperty = entry.Property("Id");

        if (idProperty != null && idProperty.CurrentValue is long idValue && idValue == 0)
        {
            idProperty.CurrentValue = idGenerator.GenerateId();
        }
    }

    private void SetPublicIdIfNeeded(EntityEntry entry)
    {
        if (entry.Metadata.FindProperty("PublicId") is not null)
        {
            var publicIdProperty = entry.Property("PublicId");

            if (publicIdProperty.CurrentValue is Ulid publicIdValue && publicIdValue == Ulid.Empty)
            {
                publicIdProperty.CurrentValue = Ulid.NewUlid();
            }
        }
    }

    private void SetAuditOnCreate(EntityEntry entry, DateTimeOffset currentTime)
    {
        entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue =
            currentUser.Id?.ToString() ?? ANONYMOUS_CREATED_BY;

        entry.Property(nameof(DefaultEntity.CreatedAt)).CurrentValue = currentTime;
    }

    private void SetAuditOnUpdate(EntityEntry entry, DateTimeOffset currentTime)
    {
        entry.Property(nameof(IAuditable.UpdatedBy)).CurrentValue =
            currentUser.Id?.ToString() ?? ANONYMOUS_CREATED_BY;

        entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = currentTime;
    }
}
