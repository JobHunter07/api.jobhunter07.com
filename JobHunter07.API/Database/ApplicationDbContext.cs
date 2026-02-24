using Microsoft.EntityFrameworkCore;
using JobHunter07.API.Entities;
namespace JobHunter07.API.Database;

public class ApplicationDbContext(DbContextOptions options)
 : DbContext(options)
{
    public DbSet<Book> Books { get; set; } = null!;
}

