using EDMS.MvcClient.Data;
using EDMS.MvcClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDMS.MvcClient.Controllers;

[Authorize]
public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _db;
    public DepartmentsController(ApplicationDbContext db) => _db = db;

    // Directory list
    public async Task<IActionResult> Index()
    {
        var items = await _db.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync();

        return View(items);
    }

    // Element-by-element + specialized documents table
    public async Task<IActionResult> Details(int id)
    {
        var dep = await _db.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dep == null) return NotFound();

        // Specialized view: docs relevant to a department
        var docs = await _db.Documents
            .AsNoTracking()
            .Where(d => d.DepartmentId == id)
            .Include(d => d.DocumentType) // optional, but useful
            .OrderByDescending(d => d.CreatedAtUtc)
            .Take(200)
            .ToListAsync();

        ViewBag.Documents = docs;

        return View(dep);
    }
}
