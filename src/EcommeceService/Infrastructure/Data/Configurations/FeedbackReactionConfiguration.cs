using Domain.Aggregates.Feedbacks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class FeedbackReactionConfiguration : IEntityTypeConfiguration<FeedbackReaction>
{
    public void Configure(EntityTypeBuilder<FeedbackReaction> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.CustomerId, x.FeedbackId }).IsUnique();
        builder.HasOne(x => x.Feedback).WithMany(x => x.Reactions).HasForeignKey(x => x.FeedbackId);
        builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
    }
}
