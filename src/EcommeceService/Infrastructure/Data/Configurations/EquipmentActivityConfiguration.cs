using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Aggregates.Equipments;

namespace Infrastructure.Data.Configurations
{
	public class EquipmentActivityConfiguration : IEntityTypeConfiguration<EquipmentActivity>
	{
		public void Configure(EntityTypeBuilder<EquipmentActivity> builder)
		{
			builder.HasKey(x => x.Id);
			builder.Property(x => x.LaborCost).HasColumnType("numeric");
			builder.Property(x => x.TotalCost).HasColumnType("numeric");
			builder
				.HasOne(x => x.Equipment)
				.WithMany(x => x.EquipmentActivities)
				.HasForeignKey(x => x.EquipmentId);
		}
	}
}
