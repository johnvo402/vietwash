using System.Data;
using System.Data.Common;
using System.Reflection;
using Application.Common.Interfaces.UnitOfWorks;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Shared.Kernel.Common;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Vouchers;
using Domain.Events;
using Infrastructure.IntegrationEvents;
using Infrastructure.Notifications;

namespace Infrastructure.Data;

public class TheDbContext(DbContextOptions<TheDbContext> options) : DbContext(options), IDbContext
{
    public DatabaseFacade DatabaseFacade => Database;

    private void MaterializeOrderEvents()
    {
        foreach (var entry in ChangeTracker.Entries<Order>().ToArray())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            Order order = entry.Entity;
            NotificationOutbox? notification = NotificationOutbox.FromOrder(order);
            if (
                notification is not null
                && !Set<NotificationOutbox>().Local.Any(x => x.Id == notification.Id)
            )
                Set<NotificationOutbox>().Add(notification);

            // External effects are durable intents; VoucherUsage is an internal DB effect.
            foreach (IntegrationOutbox message in IntegrationOutbox.FromOrder(order))
                if (!Set<IntegrationOutbox>().Local.Any(x => x.Id == message.Id))
                    Set<IntegrationOutbox>().Add(message);

            bool completedNow =
                order.Status == OrderStatus.Completed
                && order.UncommittedEvents
                    .OfType<UpdateStatusOrderEvent>()
                    .Any(x => x.Status == OrderStatus.Completed);
            if (
                completedNow
                && order.VoucherId is long voucherId
                && order.CustomerId is long customerId
                && !Set<VoucherUsage>().Local.Any(x => x.OrderId == order.Id)
            )
                Set<VoucherUsage>().Add(
                    new VoucherUsage(voucherId, customerId, order.Id, order.DiscountValue)
                );

            if (
                order.UncommittedEvents.Any(domainEvent =>
                    domainEvent
                        is not (UpdateStatusOrderEvent or EInvoiceEvent or CreateFundEvent)
                )
            )
                throw new InvalidOperationException("An Order domain event has no persistence policy.");

            // Every current Order event is now represented by aggregate state, an internal row,
            // or a durable integration outbox row in this same SaveChanges call.
            _ = order.DequeueUncommittedEvents();
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        MaterializeOrderEvents();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        MaterializeOrderEvents();
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
