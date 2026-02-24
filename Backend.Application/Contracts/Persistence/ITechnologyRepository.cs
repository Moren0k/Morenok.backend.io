using Backend.Domain.Entities;

namespace Backend.Application.Contracts.Persistence;

public interface ITechnologyRepository
{
    Task<Technology?> GetByIdAsync(Guid technologyId, CancellationToken cancellationToken = default);
    Task<Technology?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Technology>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAllAsync(IEnumerable<Guid> technologyIds, CancellationToken cancellationToken = default);
    Task AddAsync(Technology technology, CancellationToken cancellationToken = default);
}
