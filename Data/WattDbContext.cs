using Microsoft.EntityFrameworkCore;
using Models;

namespace Data;

public class WattDbContext(DbContextOptions<WattDbContext> options) : DbContext(options)
{
    public DbSet<Story> Stories { get; set; }
    public DbSet<Part> Parts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Story>()
            .HasMany(s => s.Parts)
            .WithOne(p => p.Story)
            .HasForeignKey(p => p.StoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
