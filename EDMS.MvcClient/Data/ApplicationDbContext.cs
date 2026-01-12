using EDMS.MvcClient.Models;
using Microsoft.EntityFrameworkCore;

namespace EDMS.MvcClient.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.Status);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.CreatedAtUtc);
    }
}
