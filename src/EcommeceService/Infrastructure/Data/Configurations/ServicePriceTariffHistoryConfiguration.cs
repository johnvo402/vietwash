using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Aggregates.Tariffs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ServicePriceTariffHistoryConfiguration
        : IEntityTypeConfiguration<ServicePriceTariffHistory>
    {
        public void Configure(EntityTypeBuilder<ServicePriceTariffHistory> builder)
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
                .WithMany(x => x.ServicePriceTariffHistories)
                .HasForeignKey(x => x.TariffId);

            builder
                .HasOne(x => x.Service)
                .WithMany(x => x.ServicePriceTariffHistories)
                .HasForeignKey(x => x.ServiceId);
        }
    }
}
