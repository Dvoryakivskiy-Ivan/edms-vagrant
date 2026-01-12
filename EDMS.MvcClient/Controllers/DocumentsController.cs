using EDMS.MvcClient.Data;
using EDMS.MvcClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDMS.MvcClient.Controllers;

[Authorize] 
public class DocumentsController : Controller
{
    private readonly ApplicationDbContext _db;

    public DocumentsController(ApplicationDbContext db)
    {
        _db = db;
    }

   
    [HttpGet]
    public async Task<IActionResult> Browse(
        string? q,
        DocumentStatus? status,
        string sort = "date_desc")
    {
        var isAdmin = User.IsInRole("Admin");

        IQueryable<Document> query = _db.Documents.AsNoTracking();

       
        if (!isAdmin)
            query = query.Where(d => d.Status == DocumentStatus.Approved);

      
        if (isAdmin && status.HasValue)
            query = query.Where(d => d.Status == status.Value);

       
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(d => d.Title.Contains(q) || d.Content.Contains(q));

       
        query = sort switch
        {
            "date_asc" => query.OrderBy(d => d.CreatedAtUtc),
            "title_asc" => query.OrderBy(d => d.Title),
            "title_desc" => query.OrderByDescending(d => d.Title),
            _ => query.OrderByDescending(d => d.CreatedAtUtc) // date_desc
        };

        var items = await query.ToListAsync();

        ViewBag.Q = q;
        ViewBag.Sort = sort;
        ViewBag.Status = status;
        ViewBag.IsAdmin = isAdmin;

        return View(items);
    }

   
    // CREATE (Admin only)
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View(new Document());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Document model)
    {
        if (!ModelState.IsValid)
            return View(model);

        model.CreatedAtUtc = DateTime.UtcNow;
        model.CreatedBy = User.Identity?.Name ?? "";
        model.Status = DocumentStatus.Pending;

        _db.Documents.Add(model);
        await _db.SaveChangesAsync();

    
        return RedirectToAction(nameof(Browse), new { status = DocumentStatus.Pending });
    }


    // ADMIN: Pending list shortcut (optional)
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public Task<IActionResult> Pending(string? q, string sort = "date_desc")
        => Browse(q, DocumentStatus.Pending, sort);

   
    // ADMIN: Approve / Reject
   
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

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Browse), new { status = DocumentStatus.Pending });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? comment = null)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (doc == null) return NotFound();

        doc.Status = DocumentStatus.Rejected;
        doc.DecisionComment = string.IsNullOrWhiteSpace(comment) ? null : comment;
        doc.DecisionBy = User.Identity?.Name;
        doc.DecisionAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Browse), new { status = DocumentStatus.Pending });
    }
    // DETAILS
  
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var doc = await _db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (doc == null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && doc.Status != DocumentStatus.Approved)
            return Forbid();

        return View(doc);
    }

   
    // DELETE (Admin only)
   
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (doc == null) return NotFound();

        return View(doc);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id);
        if (doc == null) return NotFound();

        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Browse));
    }

}
