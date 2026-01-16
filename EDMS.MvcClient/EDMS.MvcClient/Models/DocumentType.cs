using System.ComponentModel.DataAnnotations;

namespace EDMS.MvcClient.Models;

public class DocumentType
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [StringLength(20)]
    public string? Prefix { get; set; } // e.g. MEM-, ORD-

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Document> Documents { get; set; } = new();
}
