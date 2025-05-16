using Domain.Aggregates.Equipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace Infrastructure.Data.Configurations;

public class MaintenanceDetailConfiguration : IEntityTypeConfiguration<MaintenanceDetail>
{
    public void Configure(EntityTypeBuilder<MaintenanceDetail> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("numeric");
        builder.Property(x => x.UnitPrice).HasColumnType("numeric");
        builder
            .HasOne(x => x.MaintenanceHistory)
            .WithMany(x => x.MaintenanceDetails)
            .HasForeignKey(x => x.MaintenanceHistoryId);
    }
}
