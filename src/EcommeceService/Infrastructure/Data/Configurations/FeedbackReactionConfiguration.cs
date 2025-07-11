using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Aggregates.Feedbacks;

namespace Infrastructure.Data.Configurations;

public class FeedbackReactionConfiguration : IEntityTypeConfiguration<FeedbackReaction>
{
	public void Configure(EntityTypeBuilder<FeedbackReaction> builder)
	{
		builder.HasKey(x => x.Id);
		builder
			.HasOne(x => x.Feedback)
			.WithMany(x => x.Reactions)
			.HasForeignKey(x => x.FeedbackId);
		builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
	}
}
