namespace EDMS.MvcClient.Api.Dtos;

// v1 = OLD COMPATIBLE shape
public class DocumentDtoV1
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Number { get; set; }
    public string Status { get; set; } = "";

    public int DepartmentId { get; set; }
    public int DocumentTypeId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
