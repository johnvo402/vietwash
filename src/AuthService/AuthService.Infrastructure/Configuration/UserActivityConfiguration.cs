
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AuthService.Domain.UserActivities;

namespace AuthService.Infrastructure.Configuration
{
    public class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
    {
        public void Configure(EntityTypeBuilder<UserActivity> builder)
        {

            builder.HasIndex(r => r.Id).HasDatabaseName("ix_user_activity_id");
        }
    }
}
