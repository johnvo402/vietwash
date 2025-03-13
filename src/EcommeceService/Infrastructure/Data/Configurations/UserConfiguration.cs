using Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations.Identity;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Id);
        builder.Property(x => x.DayOfBirth).HasColumnType("date");
        builder.Property(x => x.Username).HasColumnType("citext");
        builder.HasIndex(x => x.Username).IsUnique();
        builder.Property(x => x.Email).HasColumnType("citext");
        builder.HasIndex(x => x.Email).IsUnique();

    }
}
