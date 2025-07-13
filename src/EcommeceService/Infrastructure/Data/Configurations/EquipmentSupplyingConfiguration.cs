using Domain.Aggregates.Inventories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class EquipmentSupplyingConfiguration : IEntityTypeConfiguration<EquipmentSupplying>
{
    public void Configure(EntityTypeBuilder<EquipmentSupplying> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Price).HasColumnType("numeric");
        builder.Property(x => x.Capacity).HasColumnType("numeric");

        builder
            .HasOne(x => x.Supplier)
            .WithMany(x => x.EquipmentSupplyings)
            .HasForeignKey(x => x.SupplierId);

        builder
            .HasOne(x => x.InventoryDocument)
            .WithMany(x => x.EquipmentSupplyings)
            .HasForeignKey(x => x.InventoryDocumentId);
    }
}
