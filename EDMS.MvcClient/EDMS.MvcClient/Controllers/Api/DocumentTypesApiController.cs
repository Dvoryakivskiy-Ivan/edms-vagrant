using EDMS.MvcClient.ApiModels;
using EDMS.MvcClient.Data;
using EDMS.MvcClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EDMS.MvcClient.Controllers.Api;
[AllowAnonymous]
[ApiController]
[ApiVersion("1.0")]
[Route("api/document-types")]
[Route("api/v{version:apiVersion}/document-types")]
public class DocumentTypesApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public DocumentTypesApiController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<DocumentTypeDto>>> GetAll()
    {
        var items = await _db.DocumentTypes.AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new DocumentTypeDto(t.Id, t.Name, t.Prefix, t.CreatedAtUtc))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentTypeDto>> GetById(int id)
    {
        var t = await _db.DocumentTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (t == null) return NotFound();
        return Ok(new DocumentTypeDto(t.Id, t.Name, t.Prefix, t.CreatedAtUtc));
    }

    [HttpPost]
    public async Task<ActionResult<DocumentTypeDto>> Create(DocumentTypeDto dto)
    {
        var type = new DocumentType
        {
            Name = dto.Name.Trim(),
            Prefix = string.IsNullOrWhiteSpace(dto.Prefix) ? null : dto.Prefix.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.DocumentTypes.Add(type);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = type.Id },
            new DocumentTypeDto(type.Id, type.Name, type.Prefix, type.CreatedAtUtc));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DocumentTypeDto dto)
    {
        var type = await _db.DocumentTypes.FirstOrDefaultAsync(x => x.Id == id);
        if (type == null) return NotFound();

        type.Name = dto.Name.Trim();
        type.Prefix = string.IsNullOrWhiteSpace(dto.Prefix) ? null : dto.Prefix.Trim();

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var type = await _db.DocumentTypes.FirstOrDefaultAsync(x => x.Id == id);
        if (type == null) return NotFound();

        _db.DocumentTypes.Remove(type);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
