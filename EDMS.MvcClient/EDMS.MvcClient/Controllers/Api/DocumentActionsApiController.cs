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
[Route("api/document-actions")]
[Route("api/v{version:apiVersion}/document-actions")]
public class DocumentActionsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public DocumentActionsApiController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<DocumentActionDto>>> GetAll()
    {
        var items = await _db.DocumentActions
            .AsNoTracking()
            .OrderByDescending(a => a.PerformedAtUtc)
            .Take(500)
            .Select(a => new DocumentActionDto(
                a.Id, a.DocumentId, a.ActionType, a.PerformedAtUtc, a.PerformedBy, a.Note
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentActionDto>> GetById(int id)
    {
        var a = await _db.DocumentActions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound();

        return Ok(new DocumentActionDto(a.Id, a.DocumentId, a.ActionType, a.PerformedAtUtc, a.PerformedBy, a.Note));
    }

    [HttpPost]
    public async Task<ActionResult<DocumentActionDto>> Create(DocumentActionCreateUpdateDto dto)
    {
        var docExists = await _db.Documents.AnyAsync(d => d.Id == dto.DocumentId);
        if (!docExists) return BadRequest("DocumentId not found.");

        var a = new DocumentAction
        {
            DocumentId = dto.DocumentId,
            ActionType = dto.ActionType,
            PerformedAtUtc = dto.PerformedAtUtc ?? DateTime.UtcNow,
            PerformedBy = string.IsNullOrWhiteSpace(dto.PerformedBy) ? (User?.Identity?.Name ?? "api") : dto.PerformedBy.Trim(),
            Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim()
        };

        _db.DocumentActions.Add(a);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = a.Id },
            new DocumentActionDto(a.Id, a.DocumentId, a.ActionType, a.PerformedAtUtc, a.PerformedBy, a.Note));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DocumentActionCreateUpdateDto dto)
    {
        var a = await _db.DocumentActions.FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound();

        var docExists = await _db.Documents.AnyAsync(d => d.Id == dto.DocumentId);
        if (!docExists) return BadRequest("DocumentId not found.");

        a.DocumentId = dto.DocumentId;
        a.ActionType = dto.ActionType;
        a.PerformedAtUtc = dto.PerformedAtUtc ?? a.PerformedAtUtc;
        a.PerformedBy = string.IsNullOrWhiteSpace(dto.PerformedBy) ? a.PerformedBy : dto.PerformedBy.Trim();
        a.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.DocumentActions.FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound();

        _db.DocumentActions.Remove(a);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
