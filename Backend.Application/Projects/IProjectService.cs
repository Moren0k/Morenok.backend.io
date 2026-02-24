using Backend.Application.Projects.DTOs;

namespace Backend.Application.Projects;

public interface IProjectService
{
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, CancellationToken cancellationToken = default);
    Task<ProjectDto> UpdateProjectAsync(Guid ownerId, Guid projectId, UpdateProjectDto dto, CancellationToken cancellationToken = default);
    Task DeleteProjectAsync(Guid ownerId, Guid projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectDto>> GetProjectsForAdminAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectDto>> GetPublishedProjectsAsync(Guid ownerId, CancellationToken cancellationToken = default);
}
