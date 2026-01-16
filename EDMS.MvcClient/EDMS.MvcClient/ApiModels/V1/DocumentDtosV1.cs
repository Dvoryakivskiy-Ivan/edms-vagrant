using EDMS.MvcClient.Models;

namespace EDMS.MvcClient.ApiModels.V1;

public record DocumentDtoV1(
    int Id,
    string Title,
    string? Number,
    string Content,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DocumentStatus Status,
    int DepartmentId,
    string? DepartmentName,
    int DocumentTypeId,
    string? DocumentTypeName
);

public record DocumentCreateUpdateDtoV1(
    string Title,
    string? Number,
    string Content,
    int DepartmentId,
    int DocumentTypeId
);
