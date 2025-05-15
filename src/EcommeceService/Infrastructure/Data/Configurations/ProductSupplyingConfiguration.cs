using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Aggregates.Suppliers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ProductSupplyingConfiguration : IEntityTypeConfiguration<ProductSupplying>
    {
        public void Configure(EntityTypeBuilder<ProductSupplying> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder
                .HasOne(x => x.Units)
                .WithMany()
                .HasForeignKey(x => x.UnitId);
            builder
                .HasOne(x => x.Suppliers)
                .WithMany()
                .HasForeignKey(x => x.SupplierId);
        }
    }
}