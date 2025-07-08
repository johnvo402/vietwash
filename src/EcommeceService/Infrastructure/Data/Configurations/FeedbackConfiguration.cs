using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Aggregates.Feedbacks;

namespace Infrastructure.Data.Configurations
{
	public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
	{
		public void Configure(EntityTypeBuilder<Feedback> builder)
		{
			builder.HasKey(x => x.Id);
			builder.HasIndex(x => x.Id);
			builder.HasIndex(x => x.CustomerId);
			builder.HasIndex(x => x.ServiceId);
			builder.HasIndex(x => x.ParentId);
			builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
			builder.HasOne(x => x.Staff).WithMany().HasForeignKey(x => x.StaffId);
		}
	}
}
