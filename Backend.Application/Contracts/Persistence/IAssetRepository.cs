using Backend.Domain.Entities;

namespace Backend.Application.Contracts.Persistence;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(Guid ownerId, Guid assetId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Asset>> GetByIdsAsync(Guid ownerId, IEnumerable<Guid> assetIds, CancellationToken cancellationToken = default);
    Task AddAsync(Asset asset, CancellationToken cancellationToken = default);
    Task DeleteAsync(Asset asset, CancellationToken cancellationToken = default);
}
