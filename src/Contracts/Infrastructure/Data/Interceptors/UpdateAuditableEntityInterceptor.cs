using Application.Common.Interfaces.Services;
using Domain.Aggregates.AuditLogs.Enums;
using Domain.Aggregates.AuditLogs;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using static Grpc.Core.Metadata;
using System.Text.Json;
using Domain.Common.ElasticConfigurations;
using Elastic.Clients.Elasticsearch;
using System.Security.AccessControl;
using Serilog;
using JohnChum.SharedKernel.Domain.Common;

namespace Infrastructure.Data.Interceptors;

public class UpdateAuditableEntityInterceptor(ElasticsearchClient elasticsearchClient, ICurrentUser currentUser) : SaveChangesInterceptor
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
            UpdateAuditableEntities(elasticsearchClient, eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async void UpdateAuditableEntities(ElasticsearchClient elasticsearchClient, DbContext context)
    {
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        var entities = context.ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity || e.Entity is AggregateRoot).ToList();


        foreach (EntityEntry entry in entities)
        {
            var auditLog = new AuditLog
            {
                Entity = entry.Entity.GetType().Name,
                ActionPerformBy = currentUser.Id?.ToString() ?? ANONYMOUS_CREATED_BY,
                CreatedAt = currentTime
            };
            switch (entry.State)
            {
                case EntityState.Added:
                   
                    entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue =
                        currentUser.Id?.ToString() ?? ANONYMOUS_CREATED_BY;

                    entry.Property(nameof(DefaultEntity.CreatedAt)).CurrentValue = currentTime;

                    auditLog.Type = 0; // 🔹 Create
                    auditLog.NewValue = entry.CurrentValues.ToObject();

                    break;

                case EntityState.Modified:

                    entry.Property(nameof(IAuditable.UpdatedBy)).CurrentValue =
                        currentUser.Id?.ToString() ?? ANONYMOUS_CREATED_BY;

                    entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = currentTime;

                    auditLog.Type = 1; // 🔹 Update
                    auditLog.OldValue = entry.OriginalValues.ToObject();
                    auditLog.NewValue = entry.CurrentValues.ToObject();

                    break;

                case EntityState.Deleted:
                    auditLog.Type = 2; // 🔹 Delete
                    auditLog.OldValue = entry.OriginalValues.ToObject();
                    auditLog.NewValue = null;
                    break;
            }

            try
            {
                if (auditLog.OldValue == null && auditLog.NewValue == null)
                {
                    continue;
                }
                var response = await elasticsearchClient.IndexAsync(auditLog, index: ElsIndexExtension.GetName<AuditLog>());

                if (!response.IsSuccess())
                {

                    Log.Information(
                "Elasticsearch has been failed in index audit with {debug}",
                response.DebugInformation
            );
                }
            }
            catch (Exception ex)
            {
                Log.Information($"Elasticsearch error: {ex.Message}");
            }
        }

    }
}

