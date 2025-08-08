using Domain.Aggregates.Inventories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class InventorySupplierReceiptConfiguration
        : IEntityTypeConfiguration<InventorySupplierReceipt>
    {
        public void Configure(EntityTypeBuilder<InventorySupplierReceipt> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId);

            builder
                .HasOne(x => x.InventoryDocument)
                .WithMany(x => x.InventorySupplierReceipts)
                .HasForeignKey(x => x.InventoryDocumentId);
        }
    }
}
