using Backend.Domain.Enums;

namespace Backend.Application.Projects.DTOs;

public record CreateProjectDto(
    Guid OwnerId,
    string Name,
    string ShortDescription,
    string? LongDescription,
    string? LiveUrl,
    string? RepoUrl,
    ProjectStatus Status,
    bool IsPinned,
    int? DisplayOrder,
    Guid CoverAssetId,
    Guid? DemoVideoAssetId,
    List<Guid>? TechnologyIds
);
