using Backend.Domain.Entities;

namespace Backend.Application.Contracts.Persistence;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid ownerId, Guid projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> ListPublishedAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> ListAllAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
    Task DeleteAsync(Project project, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Technology>> GetTechnologiesForProjectAsync(Guid ownerId, Guid projectId, CancellationToken cancellationToken = default);
    Task<ILookup<Guid, Technology>> GetTechnologiesForProjectsAsync(Guid ownerId, IEnumerable<Guid> projectIds, CancellationToken cancellationToken = default);
    Task ReplaceProjectTechnologiesAsync(Guid ownerId, Guid projectId, IEnumerable<Guid> technologyIds, CancellationToken cancellationToken = default);
    Task RemoveProjectTechnologiesAsync(Guid ownerId, Guid projectId, CancellationToken cancellationToken = default);

    // Ordering helpers
    Task<int> GetMaxDisplayOrderAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsPinnedAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<Guid?> GetPinnedProjectIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task ShiftDisplayOrdersAsync(Guid ownerId, int fromOrderInclusive, int delta, Guid? excludeProjectId = null, CancellationToken cancellationToken = default);
    Task NormalizeDisplayOrdersAsync(Guid ownerId, CancellationToken cancellationToken = default);
}
