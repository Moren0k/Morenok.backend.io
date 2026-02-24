using Backend.Application.Technologies.DTOs;
using Backend.Domain.Enums;

namespace Backend.Application.Projects.DTOs;

public record ProjectDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    string ShortDescription,
    string? LongDescription,
    string? LiveUrl,
    string? RepoUrl,
    ProjectStatus Status,
    bool IsPinned,
    int DisplayOrder,
    Guid CoverAssetId,
    Guid? DemoVideoAssetId,
    string? CoverUrl,
    string? DemoVideoUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<TechnologyDto> Technologies = null!
)
{
    public IReadOnlyList<TechnologyDto> Technologies { get; init; } = Technologies ?? Array.Empty<TechnologyDto>();
}
