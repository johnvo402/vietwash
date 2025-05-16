using Domain.Aggregates.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Infrastructure.Data.Configurations
{
    public class UnitRelationConfiguration : IEntityTypeConfiguration<UnitRelation>
    {
        public void Configure(EntityTypeBuilder<UnitRelation> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
			builder.Property(x => x.Price).HasColumnType("numeric");
			builder.Property(x => x.ProcessingTime).HasColumnType("numeric");
			//builder.HasOne(x => x.Product).WithMany(x => x.UnitRelations).HasForeignKey(x => x.ReferenceId);
			builder.HasOne(x => x.Service).WithMany(x => x.UnitRelations).HasForeignKey(x => x.ReferenceId);
            
        }
    }
}
