using EDMS.MvcClient.Data;
using EDMS.MvcClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDMS.MvcClient.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly ApplicationDbContext _db;
    public SearchController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Documents(
        DateTime? createdFromUtc,
        DateTime? createdToUtc,
        DateTime? decidedFromUtc,
        DateTime? decidedToUtc,
        int[]? departmentIds,
        int[]? documentTypeIds,
        DocumentStatus? status,
        string? titleStartsWith,
        string? titleEndsWith,
        string? decidedBy
    )
    {
        // Lists (requirement: search by list of elements)
        var deps = await _db.Departments.AsNoTracking().OrderBy(d => d.Name).ToListAsync();
        var types = await _db.DocumentTypes.AsNoTracking().OrderBy(t => t.Name).ToListAsync();

        ViewBag.Departments = new MultiSelectList(deps, "Id", "Name", departmentIds);
        ViewBag.DocumentTypes = new MultiSelectList(types, "Id", "Name", documentTypeIds);

        // JOINs: Documents JOIN Departments JOIN DocumentTypes JOIN (LEFT) Approvals
        var q =
            from d in _db.Documents
            join dep in _db.Departments on d.DepartmentId equals dep.Id
            join dt in _db.DocumentTypes on d.DocumentTypeId equals dt.Id
            join a in _db.DocumentApprovals on d.Id equals a.DocumentId into approvals
            from a in approvals.DefaultIfEmpty()
            select new { d, dep, dt, a };

        // Non-admin only approved documents
        if (!User.IsInRole("Admin"))
            q = q.Where(x => x.d.Status == DocumentStatus.Approved);

        // Status list
        if (status != null)
            q = q.Where(x => x.d.Status == status);

        // Date/time search (CreatedAt)
        if (createdFromUtc != null) q = q.Where(x => x.d.CreatedAtUtc >= createdFromUtc);
        if (createdToUtc != null) q = q.Where(x => x.d.CreatedAtUtc <= createdToUtc);

        // Date/time search (Approvals.DecidedAtUtc) – dependent table
        if (decidedFromUtc != null) q = q.Where(x => x.a != null && x.a.DecidedAtUtc >= decidedFromUtc);
        if (decidedToUtc != null) q = q.Where(x => x.a != null && x.a.DecidedAtUtc <= decidedToUtc);

        // Lists of elements (directories)
        if (departmentIds is { Length: > 0 })
            q = q.Where(x => departmentIds.Contains(x.dep.Id));

        if (documentTypeIds is { Length: > 0 })
            q = q.Where(x => documentTypeIds.Contains(x.dt.Id));

        // begins-with / ends-with
        if (!string.IsNullOrWhiteSpace(titleStartsWith))
        {
            titleStartsWith = titleStartsWith.Trim();
            q = q.Where(x => x.d.Title.StartsWith(titleStartsWith));
        }
        if (!string.IsNullOrWhiteSpace(titleEndsWith))
        {
            titleEndsWith = titleEndsWith.Trim();
            q = q.Where(x => x.d.Title.EndsWith(titleEndsWith));
        }

        // dependent-table list/value
        if (!string.IsNullOrWhiteSpace(decidedBy))
        {
            decidedBy = decidedBy.Trim();
            q = q.Where(x => x.a != null && x.a.DecidedBy != null && x.a.DecidedBy.Contains(decidedBy));
        }

        // De-duplicate documents because of approvals join
        var docs = await q
            .AsNoTracking()
            .GroupBy(x => x.d.Id)
            .Select(g => g
                .OrderByDescending(x => x.d.CreatedAtUtc)
                .Select(x => new DocumentSearchRow
                {
                    Id = x.d.Id,
                    Title = x.d.Title,
                    Number = x.d.Number,
                    Status = x.d.Status,
                    CreatedAtUtc = x.d.CreatedAtUtc,
                    DepartmentName = x.dep.Name,
                    DocumentTypeName = x.dt.Name,
                    LastDecisionBy = x.d.DecisionBy,
                    LastDecisionAtUtc = x.d.DecisionAtUtc
                })
                .First())
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        // keep values in UI
        ViewBag.CreatedFromUtc = createdFromUtc;
        ViewBag.CreatedToUtc = createdToUtc;
        ViewBag.DecidedFromUtc = decidedFromUtc;
        ViewBag.DecidedToUtc = decidedToUtc;
        ViewBag.Status = status;
        ViewBag.TitleStartsWith = titleStartsWith;
        ViewBag.TitleEndsWith = titleEndsWith;
        ViewBag.DecidedBy = decidedBy;

        return View(docs);
    }

    public class DocumentSearchRow
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Number { get; set; }
        public DocumentStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string DepartmentName { get; set; } = "";
        public string DocumentTypeName { get; set; } = "";
        public string? LastDecisionBy { get; set; }
        public DateTime? LastDecisionAtUtc { get; set; }
    }
}
