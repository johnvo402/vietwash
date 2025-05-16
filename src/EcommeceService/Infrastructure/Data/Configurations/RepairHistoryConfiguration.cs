using Domain.Aggregates.Equipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class RepairHistoryConfiguration : IEntityTypeConfiguration<RepairHistory>
{
    public void Configure(EntityTypeBuilder<RepairHistory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("numeric");
        builder
            .HasOne(x => x.Equipment)
            .WithMany(x => x.RepairHistories)
            .HasForeignKey(x => x.EquipmentId);
    }
}
