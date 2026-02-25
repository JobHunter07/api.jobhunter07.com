using Microsoft.EntityFrameworkCore;
using JobHunter07.API.Entities;

namespace JobHunter07.API.Database;

public class ApplicationDbContext(DbContextOptions options)
 : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Domain).HasMaxLength(200);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.WebsiteUrl).HasMaxLength(300);
            entity.Property(e => e.LinkedInUrl).HasMaxLength(300);

            entity.HasIndex(e => e.Name)
                  .IsUnique()
                  .HasDatabaseName("IX_Companies_Name_CI");

            entity.HasIndex(e => e.Domain)
                  .IsUnique()
                  .HasFilter("[Domain] IS NOT NULL")
                  .HasDatabaseName("IX_Companies_Domain");
        });
    }
}

