using EDMS.MvcClient.ApiModels;
using EDMS.MvcClient.Data;
using EDMS.MvcClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDMS.MvcClient.Controllers.Api.V1;

[AllowAnonymous]
[ApiController]
[ApiVersion("1.0")]
// OLD route (compat) + NEW versioned route
[Route("api/departments")]
[Route("api/v{version:int}/departments")]
public class DepartmentsApiV1Controller : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public DepartmentsApiV1Controller(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll()
    {
        var items = await _db.Departments.AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(d.Id, d.Name, d.Code, d.CreatedAtUtc))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var d = await _db.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound();

        return Ok(new DepartmentDto(d.Id, d.Name, d.Code, d.CreatedAtUtc));
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create(DepartmentDto dto)
    {
        var dep = new Department
        {
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Departments.Add(dep);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById),
            new { id = dep.Id, version = 1 },
            new DepartmentDto(dep.Id, dep.Name, dep.Code, dep.CreatedAtUtc));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DepartmentDto dto)
    {
        var dep = await _db.Departments.FirstOrDefaultAsync(x => x.Id == id);
        if (dep == null) return NotFound();

        dep.Name = dto.Name.Trim();
        dep.Code = dto.Code.Trim();

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var dep = await _db.Departments.FirstOrDefaultAsync(x => x.Id == id);
        if (dep == null) return NotFound();

        _db.Departments.Remove(dep);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
