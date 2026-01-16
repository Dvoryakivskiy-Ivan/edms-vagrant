using EDMS.MvcClient.Models;
using Microsoft.EntityFrameworkCore;

namespace EDMS.MvcClient.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<DocumentApproval> DocumentApprovals => Set<DocumentApproval>();

    // NEW: main register table
    public DbSet<DocumentAction> DocumentActions => Set<DocumentAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Documents
        modelBuilder.Entity<Document>()
            .HasIndex(d => d.Status);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.CreatedAtUtc);

        modelBuilder.Entity<Document>()
            .HasOne(d => d.Department)
            .WithMany(dep => dep.Documents)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .HasOne(d => d.DocumentType)
            .WithMany(t => t.Documents)
            .HasForeignKey(d => d.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Approvals (dependent)
        modelBuilder.Entity<DocumentApproval>()
            .HasOne(a => a.Document)
            .WithMany(d => d.Approvals)
            .HasForeignKey(a => a.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentApproval>()
            .HasIndex(a => a.DecidedAtUtc);

        // NEW: Actions (main register, dependent on Document)
        modelBuilder.Entity<DocumentAction>()
            .HasOne(a => a.Document)
            .WithMany(d => d.Actions)
            .HasForeignKey(a => a.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentAction>()
            .HasIndex(a => a.PerformedAtUtc);

        modelBuilder.Entity<DocumentAction>()
            .HasIndex(a => a.ActionType);
        modelBuilder.Entity<Document>()
    .   Property(d => d.Amount)
    .   HasPrecision(18, 2);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.Priority);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.Confidentiality);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.DueAtUtc);


        // SEED DATA (constant for migrations)
        var seedUtc = new DateTime(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Dean's Office", Code = "DEAN", CreatedAtUtc = seedUtc },
            new Department { Id = 2, Name = "IT Department", Code = "IT", CreatedAtUtc = seedUtc }
        );

        modelBuilder.Entity<DocumentType>().HasData(
            new DocumentType { Id = 1, Name = "Memo", Prefix = "MEM-", CreatedAtUtc = seedUtc },
            new DocumentType { Id = 2, Name = "Order", Prefix = "ORD-", CreatedAtUtc = seedUtc }
        );
    }
}
