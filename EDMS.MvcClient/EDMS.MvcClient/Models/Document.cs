using System.ComponentModel.DataAnnotations;

namespace EDMS.MvcClient.Models;

public class Document
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = "";

    [StringLength(50)]
    public string? Number { get; set; }

    [Required, StringLength(5000)]
    public string Content { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [StringLength(200)]
    public string CreatedBy { get; set; } = "";

    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

    // ✅ NEW fields (more "real" document data)
    public DocumentPriority Priority { get; set; } = DocumentPriority.Normal;

    public DocumentConfidentiality Confidentiality { get; set; } = DocumentConfidentiality.Internal;

    public DateTime? DueAtUtc { get; set; } // deadline

    [StringLength(200)]
    public string? Owner { get; set; } // responsible

    public decimal? Amount { get; set; } // budget/amount

    [StringLength(200)]
    public string? Tags { get; set; } // simple tags/category

    // Directories (FK)
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a Department.")]
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a Document Type.")]
    public int DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }

    // Decision fields (optional)
    [StringLength(200)]
    public string? DecisionBy { get; set; }

    public DateTime? DecisionAtUtc { get; set; }

    [StringLength(500)]
    public string? DecisionComment { get; set; }

    // Dependent tables
    public List<DocumentApproval> Approvals { get; set; } = new();
    public List<DocumentAction> Actions { get; set; } = new();
}
