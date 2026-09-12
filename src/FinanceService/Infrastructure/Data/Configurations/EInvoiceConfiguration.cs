using Domain.Aggregates.EInvoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class EInvoiceConfiguration : IEntityTypeConfiguration<EInvoice>
    {
        public void Configure(EntityTypeBuilder<EInvoice> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder
                .HasIndex(x => x.SourceEventId)
                .IsUnique()
                .HasFilter("source_event_id IS NOT NULL");
            builder.Property(o => o.InvoiceNumber).ValueGeneratedOnAdd();
            builder.Property(o => o.OrderDate).HasColumnType("timestamp without time zone");
            builder
                .HasMany(x => x.Items)
                .WithOne(x => x.EInvoice)
                .HasForeignKey(x => x.EInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
