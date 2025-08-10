using Domain.Aggregates.Equipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
    {
        public void Configure(EntityTypeBuilder<Equipment> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Code).HasColumnType("citext");
            builder.HasIndex(x => x.Code);
            builder.Property(x => x.Price).HasColumnType("numeric");
            builder.HasMany(x => x.OrderEquipments).WithOne().HasForeignKey(x => x.EquipmentId);
        }
    }
}
