using EDMS.MvcClient.Models;

namespace EDMS.MvcClient.ApiModels;

public record DocumentActionDto(
    int Id,
    int DocumentId,
    DocumentActionType ActionType,
    DateTime PerformedAtUtc,
    string? PerformedBy,
    string? Note
);

public record DocumentActionCreateUpdateDto(
    int DocumentId,
    DocumentActionType ActionType,
    DateTime? PerformedAtUtc,
    string? PerformedBy,
    string? Note
);
