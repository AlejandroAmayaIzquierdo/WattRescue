using Microsoft.EntityFrameworkCore;
using Models;

namespace Data;

public class WattDbContext(DbContextOptions<WattDbContext> options) : DbContext(options)
{
    public DbSet<Story> Stories { get; set; }
    public DbSet<Parts> Parts { get; set; }
    public DbSet<Paragraphs> Paragraphs { get; set; }
}
