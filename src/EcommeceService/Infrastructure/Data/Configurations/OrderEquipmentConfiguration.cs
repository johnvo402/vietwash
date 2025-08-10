using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class OrderEquipmentConfiguration : IEntityTypeConfiguration<OrderEquipment>
    {
        public void Configure(EntityTypeBuilder<OrderEquipment> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);

            builder
                .HasOne(x => x.Order)
                .WithMany(x => x.OrderEquipments)
                .HasForeignKey(x => x.OrderId);
            builder
                .HasOne(x => x.Equipment)
                .WithMany(x => x.OrderEquipments)
                .HasForeignKey(x => x.EquipmentId);
        }
    }
}
