using MediaService.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaService.Data;

public class MediaDbContext : DbContext
{
    public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options)
    {
    }

    public DbSet<Photo> Photos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Photo>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.MemberId);
            entity.HasIndex(p => p.IsApproved);
        });
    }
}

