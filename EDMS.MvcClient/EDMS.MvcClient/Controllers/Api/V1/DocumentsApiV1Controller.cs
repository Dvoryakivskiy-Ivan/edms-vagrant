using EDMS.MvcClient.ApiModels;
using EDMS.MvcClient.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;



namespace EDMS.MvcClient.Controllers.Api.V1;
[AllowAnonymous]
[ApiController]
[ApiVersion("1.0")]
// OLD route (compat) + NEW versioned route
[Route("api/documents")]
[Route("api/v{version:int}/documents")]

public class DocumentsApiV1Controller : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public DocumentsApiV1Controller(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<DocumentDto>>> GetAll()
    {
        var items = await _db.Documents.AsNoTracking()
            .Include(d => d.Department)
            .Include(d => d.DocumentType)
            .OrderByDescending(d => d.CreatedAtUtc)
            .Select(d => new DocumentDto(
                d.Id,
                d.Title,
                d.Number,
                d.Content,
                d.CreatedAtUtc,
                d.CreatedBy,
                d.Status,
                d.Priority,
                d.Confidentiality,
                d.DueAtUtc,
                d.Owner,
                d.Amount,
                d.Tags,
                d.DepartmentId,
                d.Department != null ? d.Department.Name : null,
                d.DocumentTypeId,
                d.DocumentType != null ? d.DocumentType.Name : null
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentDto>> GetById(int id)
    {
        var d = await _db.Documents.AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.DocumentType)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (d == null) return NotFound();

        return Ok(new DocumentDto(
            d.Id,
            d.Title,
            d.Number,
            d.Content,
            d.CreatedAtUtc,
            d.CreatedBy,
            d.Status,
            d.Priority,
            d.Confidentiality,
            d.DueAtUtc,
            d.Owner,
            d.Amount,
            d.Tags,
            d.DepartmentId,
            d.Department != null ? d.Department.Name : null,
            d.DocumentTypeId,
            d.DocumentType != null ? d.DocumentType.Name : null
        ));
    }
}
