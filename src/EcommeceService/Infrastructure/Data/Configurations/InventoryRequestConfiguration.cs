using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Inventories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    class InventoryRequestConfiguration : IEntityTypeConfiguration<InventoryRequest>
    {
        public void Configure(EntityTypeBuilder<InventoryRequest> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);

        }
    }
}
