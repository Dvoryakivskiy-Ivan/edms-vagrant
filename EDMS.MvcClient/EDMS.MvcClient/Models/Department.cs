using System.ComponentModel.DataAnnotations;

namespace EDMS.MvcClient.Models;

public class Department
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [Required, StringLength(20)]
    public string Code { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Document> Documents { get; set; } = new();
}
