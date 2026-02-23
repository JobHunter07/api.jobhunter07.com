using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitectureTemplate.Entities;
namespace VerticalSliceArchitectureTemplate.Database;

public class ApplicationDbContext(DbContextOptions options)
 : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = null!;
}

