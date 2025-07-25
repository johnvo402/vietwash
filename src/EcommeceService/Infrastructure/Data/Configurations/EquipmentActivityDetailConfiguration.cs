using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Aggregates.Equipments;

namespace Infrastructure.Data.Configurations
{
	public class EquipmentActivityDetailConfiguration : IEntityTypeConfiguration<EquipmentActivityDetail>
	{
		public void Configure(EntityTypeBuilder<EquipmentActivityDetail> builder)
		{
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Amount).HasColumnType("numeric");
			builder.Property(x => x.UnitPrice).HasColumnType("numeric");
			builder
				.HasOne(x => x.EquipmentActivity)
				.WithMany(x => x.ActivityDetails)
				.HasForeignKey(x => x.EquipmentActivityId);
		}
	}
}
