namespace EDMS.MvcClient.Api.Dtos;

// v2 = NEW IMPROVED shape
public class DocumentDtoV2
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public string? Number { get; set; }

    public string Status { get; set; } = "";

    // v2 includes readable names (better for UI clients)
    public string DepartmentName { get; set; } = "";
    public string DocumentTypeName { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; }
}
