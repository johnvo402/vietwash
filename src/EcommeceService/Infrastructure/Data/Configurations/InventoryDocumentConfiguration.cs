using Domain.Aggregates.Inventories;
using Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Configurations
{
    class InventoryDocumentConfiguration : IEntityTypeConfiguration<InventoryDocument>
    {
        public void Configure(
            Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<InventoryDocument> builder
        )
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.Property(x => x.Amount).HasColumnType("numeric");
            builder.Property(x => x.Code).HasColumnType("citext");
            builder
                .HasOne<Order>()
                .WithMany()
                .HasForeignKey(x => x.SourceOrderId)
                .OnDelete(DeleteBehavior.Restrict);
            builder
                .HasIndex(x => x.SourceOrderId)
                .IsUnique()
                .HasFilter("source_order_id IS NOT NULL");
        }
    }
}
