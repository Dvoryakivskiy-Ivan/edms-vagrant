using EDMS.MvcClient.Models;

namespace EDMS.MvcClient.ApiModels;

public record DocumentDto(
    int Id,
    string Title,
    string? Number,
    string Content,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DocumentStatus Status,
    DocumentPriority Priority,
    DocumentConfidentiality Confidentiality,
    DateTime? DueAtUtc,
    string? Owner,
    decimal? Amount,
    string? Tags,
    int DepartmentId,
    string? DepartmentName,
    int DocumentTypeId,
    string? DocumentTypeName
);

public record DocumentCreateUpdateDto(
    string Title,
    string? Number,
    string Content,
    DocumentPriority Priority,
    DocumentConfidentiality Confidentiality,
    DateTime? DueAtUtc,
    string? Owner,
    decimal? Amount,
    string? Tags,
    int DepartmentId,
    int DocumentTypeId
);
