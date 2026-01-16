using EDMS.MvcClient.ApiModels.V2;
using EDMS.MvcClient.Data;
using EDMS.MvcClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDMS.MvcClient.Controllers.Api.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:int}/documents")]


public class DocumentsApiV2Controller : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public DocumentsApiV2Controller(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<DocumentDtoV2>>> GetAll()
    {
        var items = await _db.Documents.AsNoTracking()
            .Include(d => d.Department)
            .Include(d => d.DocumentType)
            .OrderByDescending(d => d.CreatedAtUtc)
            .Select(d => new DocumentDtoV2(
                d.Id, d.Title, d.Number, d.Content, d.CreatedAtUtc, d.CreatedBy, d.Status,
                d.Priority, d.Confidentiality, d.DueAtUtc, d.Owner, d.Amount, d.Tags,
                new DirectoryRefDto(d.DepartmentId, d.Department!.Name),
                new DirectoryRefDto(d.DocumentTypeId, d.DocumentType!.Name)
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentDtoV2>> GetById(int id)
    {
        var d = await _db.Documents.AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.DocumentType)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (d == null) return NotFound();

        return Ok(new DocumentDtoV2(
            d.Id, d.Title, d.Number, d.Content, d.CreatedAtUtc, d.CreatedBy, d.Status,
            d.Priority, d.Confidentiality, d.DueAtUtc, d.Owner, d.Amount, d.Tags,
            new DirectoryRefDto(d.DepartmentId, d.Department?.Name ?? "(unknown)"),
            new DirectoryRefDto(d.DocumentTypeId, d.DocumentType?.Name ?? "(unknown)")
        ));
    }

    [HttpPost]
    public async Task<ActionResult<DocumentDtoV2>> Create(DocumentCreateUpdateDtoV2 dto)
    {
        if (!await _db.Departments.AnyAsync(x => x.Id == dto.DepartmentId))
            return BadRequest("DepartmentId not found.");
        if (!await _db.DocumentTypes.AnyAsync(x => x.Id == dto.DocumentTypeId))
            return BadRequest("DocumentTypeId not found.");

        var doc = new Document
        {
            Title = dto.Title.Trim(),
            Number = string.IsNullOrWhiteSpace(dto.Number) ? null : dto.Number.Trim(),
            Content = dto.Content,

            Priority = dto.Priority,
            Confidentiality = dto.Confidentiality,
            DueAtUtc = dto.DueAtUtc,
            Owner = string.IsNullOrWhiteSpace(dto.Owner) ? null : dto.Owner.Trim(),
            Amount = dto.Amount,
            Tags = string.IsNullOrWhiteSpace(dto.Tags) ? null : dto.Tags.Trim(),

            DepartmentId = dto.DepartmentId,
            DocumentTypeId = dto.DocumentTypeId,

            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = User?.Identity?.Name ?? "api",
            Status = DocumentStatus.Pending
        };

        // optional auto-number using prefix
        if (string.IsNullOrWhiteSpace(doc.Number))
        {
            var type = await _db.DocumentTypes.AsNoTracking().FirstAsync(t => t.Id == doc.DocumentTypeId);
            if (!string.IsNullOrWhiteSpace(type.Prefix))
                doc.Number = $"{type.Prefix}{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();

        var created = await _db.Documents.AsNoTracking()
            .Include(d => d.Department)
            .Include(d => d.DocumentType)
            .FirstAsync(d => d.Id == doc.Id);

        return CreatedAtAction(nameof(GetById),
            new { id = created.Id, version = "2.0" },
            new DocumentDtoV2(
                created.Id, created.Title, created.Number, created.Content, created.CreatedAtUtc, created.CreatedBy, created.Status,
                created.Priority, created.Confidentiality, created.DueAtUtc, created.Owner, created.Amount, created.Tags,
                new DirectoryRefDto(created.DepartmentId, created.Department!.Name),
                new DirectoryRefDto(created.DocumentTypeId, created.DocumentType!.Name)
            ));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DocumentCreateUpdateDtoV2 dto)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (doc == null) return NotFound();

        if (!await _db.Departments.AnyAsync(x => x.Id == dto.DepartmentId))
            return BadRequest("DepartmentId not found.");
        if (!await _db.DocumentTypes.AnyAsync(x => x.Id == dto.DocumentTypeId))
            return BadRequest("DocumentTypeId not found.");

        doc.Title = dto.Title.Trim();
        doc.Number = string.IsNullOrWhiteSpace(dto.Number) ? null : dto.Number.Trim();
        doc.Content = dto.Content;

        doc.Priority = dto.Priority;
        doc.Confidentiality = dto.Confidentiality;
        doc.DueAtUtc = dto.DueAtUtc;
        doc.Owner = string.IsNullOrWhiteSpace(dto.Owner) ? null : dto.Owner.Trim();
        doc.Amount = dto.Amount;
        doc.Tags = string.IsNullOrWhiteSpace(dto.Tags) ? null : dto.Tags.Trim();

        doc.DepartmentId = dto.DepartmentId;
        doc.DocumentTypeId = dto.DocumentTypeId;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (doc == null) return NotFound();

        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
