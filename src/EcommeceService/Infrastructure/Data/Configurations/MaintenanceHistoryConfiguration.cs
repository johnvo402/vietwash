using Domain.Aggregates.Equipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class MaintenanceHistoryConfiguration : IEntityTypeConfiguration<MaintenanceHistory>
{
    public void Configure(EntityTypeBuilder<MaintenanceHistory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Total).HasColumnType("numeric");
        builder
            .HasOne(x => x.Equipment)
            .WithMany(x => x.MaintenanceHistories)
            .HasForeignKey(x => x.EquipmentId);
    }
}
