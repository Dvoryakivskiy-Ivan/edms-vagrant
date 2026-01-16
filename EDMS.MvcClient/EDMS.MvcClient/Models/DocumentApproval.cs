using System.ComponentModel.DataAnnotations;

namespace EDMS.MvcClient.Models;

public class DocumentApproval
{
    public int Id { get; set; }

    public int DocumentId { get; set; }
    public Document Document { get; set; } = default!;

    [Required, StringLength(20)]
    public string Decision { get; set; } = "Approve"; // Approve / Reject

    public DateTime DecidedAtUtc { get; set; } = DateTime.UtcNow;

    [StringLength(200)]
    public string? DecidedBy { get; set; }

    [StringLength(500)]
    public string? Comment { get; set; }
}
