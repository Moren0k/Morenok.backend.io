using Backend.Application.Contracts.Persistence;
using Backend.Domain.Entities;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

public class TechnologyRepository : ITechnologyRepository
{
    private readonly AppDbContext _context;

    public TechnologyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Technology technology, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(technology.Name))
            throw new ArgumentException("Name must not be null or empty.");

        if (string.IsNullOrWhiteSpace(technology.Slug))
            throw new ArgumentException("Slug must not be null or empty.");

        var lowerSlug = technology.Slug.ToLowerInvariant();
        if (technology.Slug != lowerSlug)
            throw new ArgumentException("Slug must be lowercase.");

        bool slugExists = await _context.Technologies.AnyAsync(t => t.Slug == technology.Slug, cancellationToken);
        if (slugExists)
            throw new InvalidOperationException($"A technology with slug '{technology.Slug}' already exists.");

        bool nameExists = await _context.Technologies.AnyAsync(
            t => t.Name.ToLower() == technology.Name.ToLower(), cancellationToken);
        if (nameExists)
            throw new InvalidOperationException($"A technology with name '{technology.Name}' already exists.");

        await _context.Technologies.AddAsync(technology, cancellationToken);
    }

    public Task<Technology?> GetByIdAsync(Guid technologyId, CancellationToken cancellationToken = default)
    {
        return _context.Technologies.FirstOrDefaultAsync(t => t.Id == technologyId, cancellationToken);
    }

    public Task<Technology?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _context.Technologies.FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Technology>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Technologies
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAllAsync(IEnumerable<Guid> technologyIds, CancellationToken cancellationToken = default)
    {
        if (technologyIds == null || !technologyIds.Any()) return true;
        
        var distinctIds = technologyIds.Distinct().ToList();
        var foundCount = await _context.Technologies
            .Where(t => distinctIds.Contains(t.Id))
            .CountAsync(cancellationToken);
            
        return foundCount == distinctIds.Count;
    }
}
