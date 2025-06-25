using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Inventories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    class InventoryRelationConfiguration : IEntityTypeConfiguration<InventoryRelation>
    {
        public void Configure(EntityTypeBuilder<InventoryRelation> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.Property(x => x.Amount).HasColumnType("numeric");
            builder.HasOne(x => x.InventoryInvoice).WithMany(x => x.InventoryRelationships).HasForeignKey(x => x.InventoryInvoiceId);
            builder.HasOne(x => x.InventoryDocument).WithMany(x => x.InventoryRelationships).HasForeignKey(x => x.InventoryDocumentId);
        }
    }
}
