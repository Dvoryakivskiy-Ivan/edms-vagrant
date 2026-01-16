using EDMS.MvcClient.Models;

namespace EDMS.MvcClient.Data;

public static class DbSeeder
{
    // Seeds initial demo data once (idempotent)
    public static void Seed(ApplicationDbContext db)
    {
        // 1) Make sure directories exist (in case migration seed didn't run for some reason)
        if (!db.Departments.Any())
        {
            db.Departments.AddRange(
                new Department
                {
                    Id = 1,
                    Name = "Dean's Office",
                    Code = "DEAN",
                    CreatedAtUtc = new DateTime(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc)
                },
                new Department
                {
                    Id = 2,
                    Name = "IT Department",
                    Code = "IT",
                    CreatedAtUtc = new DateTime(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }

        if (!db.DocumentTypes.Any())
        {
            db.DocumentTypes.AddRange(
                new DocumentType
                {
                    Id = 1,
                    Name = "Memo",
                    Prefix = "MEM-",
                    CreatedAtUtc = new DateTime(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc)
                },
                new DocumentType
                {
                    Id = 2,
                    Name = "Order",
                    Prefix = "ORD-",
                    CreatedAtUtc = new DateTime(2026, 1, 14, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }

        // Save directories first so IDs exist
        db.SaveChanges();

        // 2) If there is already data, do nothing
        if (db.Documents.Any())
            return;

       
        var depId = db.Departments.OrderBy(d => d.Id).Select(d => d.Id).First();
        var typeId = db.DocumentTypes.OrderBy(t => t.Id).Select(t => t.Id).First();

        var now = DateTime.UtcNow;

        db.Documents.AddRange(
            new Document
            {
                Title = "Welcome document",
                Content = "This document is seeded automatically.",
                CreatedAtUtc = now.AddDays(-2),
                CreatedBy = "system",
                Status = DocumentStatus.Approved,
                DecisionBy = "system",
                DecisionAtUtc = now.AddDays(-2),
                DecisionComment = "Seeded",

                // REQUIRED FKs
                DepartmentId = depId,
                DocumentTypeId = typeId
            },
            new Document
            {
                Title = "Pending example",
                Content = "Pending document seeded for demo.",
                CreatedAtUtc = now.AddDays(-1),
                CreatedBy = "system",
                Status = DocumentStatus.Pending,

                // REQUIRED FKs
                DepartmentId = depId,
                DocumentTypeId = typeId
            }
        );

        db.SaveChanges();
    }
}
