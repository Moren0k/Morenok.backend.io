using Backend.Application.Contracts.Persistence;
using Backend.Domain.Entities;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly AppDbContext _context;

    public AssetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        await _context.Assets.AddAsync(asset, cancellationToken);
    }

    public Task DeleteAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        _context.Assets.Remove(asset);
        return Task.CompletedTask;
    }

    public Task<Asset?> GetByIdAsync(Guid ownerId, Guid assetId, CancellationToken cancellationToken = default)
    {
        return _context.Assets.FirstOrDefaultAsync(a => a.OwnerId == ownerId && a.Id == assetId, cancellationToken);
    }

    public async Task<IReadOnlyList<Asset>> GetByIdsAsync(Guid ownerId, IEnumerable<Guid> assetIds, CancellationToken cancellationToken = default)
    {
        var idList = assetIds.Distinct().ToList();
        return await _context.Assets
            .AsNoTracking()
            .Where(a => a.OwnerId == ownerId && idList.Contains(a.Id))
            .ToListAsync(cancellationToken);
    }
}
