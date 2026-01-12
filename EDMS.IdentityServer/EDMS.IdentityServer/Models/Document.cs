namespace EDMS.MvcClient.Models;

public enum DocumentStatus
{
    Draft,
    PendingApproval,
    Approved,
    Rejected
}

public class Document
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Type { get; set; } = "";
    public string Author { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public string Content { get; set; } = "";
}
