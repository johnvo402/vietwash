using Domain.Aggregates.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Id);
            builder.HasOne(x => x.Template).WithMany().HasForeignKey(x => x.TemplateId);
            builder.Property(x => x.Data).HasColumnType("hstore");
            builder.Property(x => x.Parameters).HasColumnType("hstore");
        }
    }
}
