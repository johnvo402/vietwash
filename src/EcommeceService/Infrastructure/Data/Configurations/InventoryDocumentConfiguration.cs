using Domain.Aggregates.Inventories;
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
        }
    }
}
