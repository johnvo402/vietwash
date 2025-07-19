using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Vouchers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.Property(x => x.Code).HasColumnType("citext");
            builder.Property(x => x.Amount).HasColumnType("numeric");
            builder.Property(x => x.Total).HasColumnType("numeric");
            builder.Property(x => x.DiscountValue).HasColumnType("numeric");
            builder.HasIndex(x => x.CustomerId);
            builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
            builder.HasOne(x => x.Staff).WithMany().HasForeignKey(x => x.StaffId);
        }
    }
}
