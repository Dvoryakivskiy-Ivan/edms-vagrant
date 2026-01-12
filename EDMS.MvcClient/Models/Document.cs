using System.ComponentModel.DataAnnotations;

namespace EDMS.MvcClient.Models;

public class Document
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = "";

    [Required, StringLength(5000)]
    public string Content { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

  
    [StringLength(200)]
    public string CreatedBy { get; set; } = "";

    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

   
    [StringLength(200)]
    public string? DecisionBy { get; set; }

    public DateTime? DecisionAtUtc { get; set; }

    [StringLength(500)]
    public string? DecisionComment { get; set; }
}
