using Microsoft.EntityFrameworkCore;
using ProjectService.Domain.Entity;
namespace ProjectService.Infrastructure.Persistence;

public class ProjectDbContext : DbContext
{
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationSetting> OrganizationSettings { get; set; }

    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasOne(e => e.Setting).WithOne(e => e.Organization).HasForeignKey<OrganizationSetting>(e => e.OrgId);
        });
        modelBuilder.Entity<OrganizationSetting>(entity =>
        {
            entity.HasOne(e => e.Organization).WithOne(e => e.Setting).HasForeignKey<OrganizationSetting>(e => e.OrgId);
        });
    }
}

