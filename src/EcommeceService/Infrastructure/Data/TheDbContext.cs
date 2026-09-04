using System.Data;
using System.Data.Common;
using System.Reflection;
using Application.Common.Interfaces.UnitOfWorks;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Shared.Kernel.Common;
using Domain.Aggregates.Orders;
using Infrastructure.Notifications;

namespace Infrastructure.Data;

public class TheDbContext(DbContextOptions<TheDbContext> options) : DbContext(options), IDbContext
{
    public DatabaseFacade DatabaseFacade => Database;

    private void CaptureNotifications()
    {
        foreach (var entry in ChangeTracker.Entries<Order>().ToArray())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;
            var message = NotificationOutbox.FromOrder(entry.Entity);
            if (message != null && !Set<NotificationOutbox>().Local.Any(x => x.Id == message.Id))
                Set<NotificationOutbox>().Add(message);
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        CaptureNotifications();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        CaptureNotifications();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override DbSet<TEntity> Set<TEntity>()
        where TEntity : class => base.Set<TEntity>();

    public async Task UseTransactionAsync(DbTransaction transaction)
    {
        DbConnection dbConnection = Database.GetDbConnection();

        if (dbConnection.State == ConnectionState.Closed)
        {
            dbConnection.Open();
        }

        Guard.Against.Null(transaction, nameof(transaction), "transaction is not null");

        if (transaction.Connection != dbConnection)
        {
            throw new Exception("Cannot share transaction with difference connections");
        }

        await Database.UseTransactionAsync(transaction);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.HasPostgresExtension("citext");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSnakeCaseNamingConvention();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<Ulid>().HaveConversion<UlidToStringConverter>();
}
