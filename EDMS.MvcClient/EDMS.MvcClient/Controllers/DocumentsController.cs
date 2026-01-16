using EDMS.MvcClient.Data;
using EDMS.MvcClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDMS.MvcClient.Controllers;

[Authorize]
public class DocumentsController : Controller
{
    private readonly ApplicationDbContext _db;

    public DocumentsController(ApplicationDbContext db) => _db = db;

    // GET: /Documents/Browse?status=Pending&q=...
    [HttpGet]
    public async Task<IActionResult> Browse(string? q, DocumentStatus? status, string sort = "date_desc")
    {
        var query = _db.Documents
            .AsNoTracking()
            .Include(d => d.Department)
            .Include(d => d.DocumentType)
            .AsQueryable();

        // Non-admin sees only Approved
        if (!User.IsInRole("Admin"))
            query = query.Where(d => d.Status == DocumentStatus.Approved);
        else if (status != null)
            query = query.Where(d => d.Status == status);

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(d =>
                d.Title.Contains(q) ||
                d.Content.Contains(q) ||
                (d.Number != null && d.Number.Contains(q)) ||
                (d.Department != null && d.Department.Name.Contains(q)) ||
                (d.DocumentType != null && d.DocumentType.Name.Contains(q)));
        }

        query = sort switch
        {
            "date_asc" => query.OrderBy(d => d.CreatedAtUtc),
            "title_asc" => query.OrderBy(d => d.Title),
            "title_desc" => query.OrderByDescending(d => d.Title),
            _ => query.OrderByDescending(d => d.CreatedAtUtc),
        };

        ViewBag.Q = q;
        ViewBag.Status = status;
        ViewBag.Sort = sort;

        return View(await query.ToListAsync());
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public Task<IActionResult> Pending(string? q, string sort = "date_desc")
        => Browse(q, DocumentStatus.Pending, sort);

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var doc = await _db.Documents
    .AsNoTracking()
    .Include(d => d.Department)
    .Include(d => d.DocumentType)
    .Include(d => d.Approvals.OrderByDescending(a => a.DecidedAtUtc))
    .Include(d => d.Actions.OrderByDescending(a => a.PerformedAtUtc))
    .FirstOrDefaultAsync(d => d.Id == id);


        if (doc == null) return NotFound();

        // Non-admin can't open non-approved
        if (!User.IsInRole("Admin") && doc.Status != DocumentStatus.Approved)
            return Forbid();

        return View(doc);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await FillDirectoryDropdowns();
        return View(new Document());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Document model)
    {
        if (!ModelState.IsValid)
        {
            await FillDirectoryDropdowns(model.DepartmentId, model.DocumentTypeId);
            return View(model);
        }

        model.CreatedAtUtc = DateTime.UtcNow;
        model.CreatedBy = User.Identity?.Name ?? "";
        model.Status = DocumentStatus.Pending;

        // auto-generate a Number from type prefix (optional)
        var type = await _db.DocumentTypes.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == model.DocumentTypeId);

        if (type != null && !string.IsNullOrWhiteSpace(type.Prefix) && string.IsNullOrWhiteSpace(model.Number))
        {
            model.Number = $"{type.Prefix}{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        _db.Documents.Add(model);
        await _db.SaveChangesAsync(); // model.Id is available after this

        // NEW: log action
        _db.DocumentActions.Add(new DocumentAction
        {
            DocumentId = model.Id,
            ActionType = DocumentActionType.Created,
            PerformedBy = User.Identity?.Name ?? "unknown",
            PerformedAtUtc = DateTime.UtcNow,
            Note = "Document created"
        });
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Browse), new { status = DocumentStatus.Pending });
    }


    // ADMIN approve/reject (writes to dependent table)
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (doc == null) return NotFound();

        doc.Status = DocumentStatus.Approved;
        doc.DecisionBy = User.Identity?.Name;
        doc.DecisionAtUtc = DateTime.UtcNow;
        doc.DecisionComment = null;

        _db.DocumentApprovals.Add(new DocumentApproval
        {
            DocumentId = doc.Id,
            Decision = "Approve",
            DecidedAtUtc = doc.DecisionAtUtc.Value,
            DecidedBy = doc.DecisionBy
        });

        // NEW: log action
        _db.DocumentActions.Add(new DocumentAction
        {
            DocumentId = doc.Id,
            ActionType = DocumentActionType.Approved,
            PerformedBy = User.Identity?.Name ?? "unknown",
            PerformedAtUtc = DateTime.UtcNow,
            Note = "Approved"
        });

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Browse), new { status = DocumentStatus.Pending });
    }


    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? comment)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (doc == null) return NotFound();

        doc.Status = DocumentStatus.Rejected;
        doc.DecisionBy = User.Identity?.Name;
        doc.DecisionAtUtc = DateTime.UtcNow;
        doc.DecisionComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

        _db.DocumentApprovals.Add(new DocumentApproval
        {
            DocumentId = doc.Id,
            Decision = "Reject",
            DecidedAtUtc = doc.DecisionAtUtc.Value,
            DecidedBy = doc.DecisionBy,
            Comment = doc.DecisionComment
        });

        // NEW: log action
        _db.DocumentActions.Add(new DocumentAction
        {
            DocumentId = doc.Id,
            ActionType = DocumentActionType.Rejected,
            PerformedBy = User.Identity?.Name ?? "unknown",
            PerformedAtUtc = DateTime.UtcNow,
            Note = string.IsNullOrWhiteSpace(doc.DecisionComment) ? "Rejected" : $"Rejected: {doc.DecisionComment}"
        });

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Browse), new { status = DocumentStatus.Pending });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id);
        if (doc == null) return NotFound();

        // NEW: log action BEFORE deleting the document
        _db.DocumentActions.Add(new DocumentAction
        {
            DocumentId = doc.Id,
            ActionType = DocumentActionType.Deleted,
            PerformedBy = User.Identity?.Name ?? "unknown",
            PerformedAtUtc = DateTime.UtcNow,
            Note = "Deleted"
        });
        await _db.SaveChangesAsync();

        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Browse));
    }


    private async Task FillDirectoryDropdowns(int? departmentId = null, int? documentTypeId = null)
    {
        var deps = await _db.Departments.AsNoTracking().OrderBy(d => d.Name).ToListAsync();
        var types = await _db.DocumentTypes.AsNoTracking().OrderBy(t => t.Name).ToListAsync();

        ViewBag.Departments = new SelectList(deps, "Id", "Name", departmentId);
        ViewBag.DocumentTypes = new SelectList(types, "Id", "Name", documentTypeId);
    }
}
