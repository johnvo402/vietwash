using Domain.Aggregates.Funds;
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
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(x => x.BirthDay).HasColumnType("date");
        builder.Property(x => x.Email).HasColumnType("citext");
        builder.HasIndex(x => x.Email).IsUnique();
        builder
            .HasMany(x => x.BranchUsers)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
