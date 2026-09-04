using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data;

// Retained after notifications are read/deleted so a late retry cannot recreate them.
public sealed class NotificationReceipt
{
    public string Id { get; set; } = null!;
    public string RecipientKey { get; set; } = null!;
    public DateTimeOffset AcceptedAt { get; set; }
}

public sealed class NotificationReceiptConfiguration : IEntityTypeConfiguration<NotificationReceipt>
{
    public void Configure(EntityTypeBuilder<NotificationReceipt> builder)
    {
        builder.ToTable("notification_receipts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(128);
        builder.Property(x => x.RecipientKey).HasMaxLength(64);
    }
}
