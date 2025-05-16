using Domain.Aggregates.Equipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class RepairDetailConfiguration : IEntityTypeConfiguration<RepairDetail>
{
    public void Configure(EntityTypeBuilder<RepairDetail> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("numeric");
        builder.Property(x => x.UnitPrice).HasColumnType("numeric");
        builder
            .HasOne(x => x.RepairHistory)
            .WithMany(x => x.RepairDetails)
            .HasForeignKey(x => x.RepairHistoryId);
    }
}
