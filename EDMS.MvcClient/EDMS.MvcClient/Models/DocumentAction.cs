using System.ComponentModel.DataAnnotations;

namespace EDMS.MvcClient.Models;

public enum DocumentActionType
{
    Created = 1,
    Approved = 2,
    Rejected = 3,
    Deleted = 4,
    Edited = 5,
    Commented = 6
}

public class DocumentAction
{
    public int Id { get; set; }

    [Required]
    public int DocumentId { get; set; }
    public Document? Document { get; set; }

    [Required]
    public DocumentActionType ActionType { get; set; }

 
    [Required, MaxLength(200)]
    public string PerformedBy { get; set; } = "";

    [Required]
    public DateTime PerformedAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Note { get; set; }
}
