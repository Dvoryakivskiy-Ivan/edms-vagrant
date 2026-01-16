using EDMS.MvcClient.Models;

namespace EDMS.MvcClient.ApiModels.V2;

public record DirectoryRefDto(int Id, string Name);

public record DocumentDtoV2(
    int Id,
    string Title,
    string? Number,
    string Content,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DocumentStatus Status,

    // NEW: richer v2 output (these fields you added earlier)
    DocumentPriority Priority,
    DocumentConfidentiality Confidentiality,
    DateTime? DueAtUtc,
    string? Owner,
    decimal? Amount,
    string? Tags,

    // NEW: cleaner nested refs (instead of separate id + name fields)
    DirectoryRefDto Department,
    DirectoryRefDto DocumentType
);

public record DocumentCreateUpdateDtoV2(
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
