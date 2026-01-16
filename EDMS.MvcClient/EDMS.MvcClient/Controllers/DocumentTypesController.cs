using EDMS.MvcClient.Data;
using EDMS.MvcClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDMS.MvcClient.Controllers;

[Authorize]
public class DocumentTypesController : Controller
{
    private readonly ApplicationDbContext _db;
    public DocumentTypesController(ApplicationDbContext db) => _db = db;

    // Directory list
    public async Task<IActionResult> Index()
    {
        var items = await _db.DocumentTypes
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();

        return View(items);
    }

    // Element-by-element + specialized documents table
    public async Task<IActionResult> Details(int id)
    {
        var t = await _db.DocumentTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (t == null) return NotFound();

        // Specialized view: docs relevant to a type
        var docs = await _db.Documents
            .AsNoTracking()
            .Where(d => d.DocumentTypeId == id)
            .Include(d => d.Department) // optional, but useful
            .OrderByDescending(d => d.CreatedAtUtc)
            .Take(200)
            .ToListAsync();

        ViewBag.Documents = docs;

        return View(t);
    }
}
