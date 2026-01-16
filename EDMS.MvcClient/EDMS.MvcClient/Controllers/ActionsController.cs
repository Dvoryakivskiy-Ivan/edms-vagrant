using EDMS.MvcClient.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDMS.MvcClient.Controllers;

[Authorize]
public class ActionsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ActionsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /Actions
    public async Task<IActionResult> Index(
        DateTime? fromUtc,
        DateTime? toUtc,
        int? departmentId,
        int? documentTypeId,
        string? titleStartsWith,
        string? titleEndsWith,
        string? performedBy,
        int? actionType // enum int
    )
    {
        var q = _db.DocumentActions
            .AsNoTracking()
            .Include(a => a.Document)!.ThenInclude(d => d.Department)
            .Include(a => a.Document)!.ThenInclude(d => d.DocumentType)
            .AsQueryable();

        // date/time search
        if (fromUtc.HasValue) q = q.Where(a => a.PerformedAtUtc >= fromUtc.Value);
        if (toUtc.HasValue) q = q.Where(a => a.PerformedAtUtc <= toUtc.Value);

        // list filters (directories)
        if (departmentId.HasValue) q = q.Where(a => a.Document != null && a.Document.DepartmentId == departmentId.Value);
        if (documentTypeId.HasValue) q = q.Where(a => a.Document != null && a.Document.DocumentTypeId == documentTypeId.Value);

        if (!string.IsNullOrWhiteSpace(titleStartsWith))
            q = q.Where(a => a.Document != null && a.Document.Title.StartsWith(titleStartsWith));

        if (!string.IsNullOrWhiteSpace(titleEndsWith))
            q = q.Where(a => a.Document != null && a.Document.Title.EndsWith(titleEndsWith));

        // performedBy contains
        if (!string.IsNullOrWhiteSpace(performedBy))
            q = q.Where(a => a.PerformedBy.Contains(performedBy));

        // action type list
        if (actionType.HasValue)
            q = q.Where(a => (int)a.ActionType == actionType.Value);

        q = q.OrderByDescending(a => a.PerformedAtUtc);

        // dropdown data
        ViewBag.Departments = await _db.Departments.AsNoTracking().OrderBy(d => d.Name).ToListAsync();
        ViewBag.DocumentTypes = await _db.DocumentTypes.AsNoTracking().OrderBy(t => t.Name).ToListAsync();

        // keep current filters in view
        ViewBag.FromUtc = fromUtc;
        ViewBag.ToUtc = toUtc;
        ViewBag.DepartmentId = departmentId;
        ViewBag.DocumentTypeId = documentTypeId;
        ViewBag.TitleStartsWith = titleStartsWith;
        ViewBag.TitleEndsWith = titleEndsWith;
        ViewBag.PerformedBy = performedBy;
        ViewBag.ActionType = actionType;

        var items = await q.Take(500).ToListAsync();
        return View(items);
    }

    // GET: /Actions/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.DocumentActions
            .AsNoTracking()
            .Include(a => a.Document)!.ThenInclude(d => d.Department)
            .Include(a => a.Document)!.ThenInclude(d => d.DocumentType)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (item == null) return NotFound();
        return View(item);
    }
}
