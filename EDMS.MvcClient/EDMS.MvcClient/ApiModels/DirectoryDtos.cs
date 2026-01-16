namespace EDMS.MvcClient.ApiModels;

public record DepartmentDto(int Id, string Name, string Code, DateTime CreatedAtUtc);
public record DocumentTypeDto(int Id, string Name, string? Prefix, DateTime CreatedAtUtc);
