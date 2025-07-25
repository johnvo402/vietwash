using Domain.Aggregates.Tariffs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ServiceTariffConfiguration : IEntityTypeConfiguration<ServiceTariff>
    {
        public void Configure(EntityTypeBuilder<ServiceTariff> builder)
        {
            builder
                .HasIndex(x => new
                {
                    x.TariffId,
                    x.ServiceId,
                    x.UnitRelationId,
                })
                .IsUnique();
            builder
                .HasOne(x => x.Tariff)
                .WithMany(x => x.ServiceTariffs)
                .HasForeignKey(x => x.TariffId);
            builder
                .HasOne(x => x.Service)
                .WithMany(x => x.ServiceTariffs)
                .HasForeignKey(x => x.ServiceId);
            builder.HasOne(x => x.UnitRelation).WithMany().HasForeignKey(x => x.UnitRelationId);
        }
    }
}
