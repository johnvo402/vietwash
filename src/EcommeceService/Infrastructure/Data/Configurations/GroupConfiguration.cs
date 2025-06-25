using Domain.Aggregates.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Infrastructure.Data.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.Property(x => x.Name).HasColumnType("citext");
            builder.Property(x => x.Description).HasColumnType("citext");
            builder.Property(x => x.Price).HasColumnType("numeric");
        }
    }
}
