using Backend.Application.Contracts.Persistence;
using Backend.Domain.Entities;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(project, cancellationToken);
    }

    public Task DeleteAsync(Project project, CancellationToken cancellationToken = default)
    {
        _context.Projects.Remove(project);
        return Task.CompletedTask;
    }

    public Task<Project?> GetByIdAsync(Guid ownerId, Guid projectId, CancellationToken cancellationToken = default)
    {
        return _context.Projects.FirstOrDefaultAsync(p => p.OwnerId == ownerId && p.Id == projectId, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> ListAllAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.OwnerId == ownerId)
            .OrderByDescending(p => p.IsPinned)
            .ThenBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> ListPublishedAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.OwnerId == ownerId && p.Status == Domain.Enums.ProjectStatus.Published)
            .OrderByDescending(p => p.IsPinned)
            .ThenBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _context.Projects.Update(project);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsPinnedAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects.AnyAsync(p => p.OwnerId == ownerId && p.IsPinned, cancellationToken);
    }

    public async Task<int> GetMaxDisplayOrderAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var max = await _context.Projects
            .Where(p => p.OwnerId == ownerId && !p.IsPinned)
            .MaxAsync(p => (int?)p.DisplayOrder, cancellationToken);
        
        return max ?? 0;
    }

    public async Task<Guid?> GetPinnedProjectIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Where(p => p.OwnerId == ownerId && p.IsPinned)
            .Select(p => new { p.Id })
            .FirstOrDefaultAsync(cancellationToken);
            
        return project?.Id;
    }

    public async Task NormalizeDisplayOrdersAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var projects = await _context.Projects
            .Where(p => p.OwnerId == ownerId && !p.IsPinned)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        for (int i = 0; i < projects.Count; i++)
        {
            int expectedOrder = i + 1;
            if (projects[i].DisplayOrder != expectedOrder)
            {
                _context.Entry(projects[i]).Property(p => p.DisplayOrder).CurrentValue = expectedOrder;
            }
        }
    }

    public async Task ShiftDisplayOrdersAsync(Guid ownerId, int fromOrderInclusive, int delta, Guid? excludeProjectId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Projects.Where(p => p.OwnerId == ownerId && !p.IsPinned && p.DisplayOrder >= fromOrderInclusive);
        
        if (excludeProjectId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProjectId.Value);
        }

        var projectsToShift = await query.ToListAsync(cancellationToken);
        
        foreach (var p in projectsToShift)
        {
            _context.Entry(p).Property(x => x.DisplayOrder).CurrentValue = p.DisplayOrder + delta;
        }
    }

    public async Task<IReadOnlyList<Technology>> GetTechnologiesForProjectAsync(Guid ownerId, Guid projectId, CancellationToken cancellationToken = default)
    {
        // First verify the project belongs to the owner
        var projectExists = await _context.Projects.AnyAsync(p => p.OwnerId == ownerId && p.Id == projectId, cancellationToken);
        if (!projectExists) return Array.Empty<Technology>();

        return await _context.ProjectTechnologies
            .Where(pt => pt.ProjectId == projectId)
             .Join(_context.Technologies,
                pt => pt.TechnologyId,
                t => t.Id,
                (pt, t) => t)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ILookup<Guid, Technology>> GetTechnologiesForProjectsAsync(Guid ownerId, IEnumerable<Guid> projectIds, CancellationToken cancellationToken = default)
    {
        var idList = projectIds.Distinct().ToList();
        if (!idList.Any()) return Array.Empty<Technology>().ToLookup(x => Guid.Empty, x => x);

        // Verify ownership for all requested IDs (must all belong to owner)
        var validProjectIds = await _context.Projects
            .Where(p => p.OwnerId == ownerId && idList.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var technologies = await _context.ProjectTechnologies
            .Where(pt => validProjectIds.Contains(pt.ProjectId))
            .Join(_context.Technologies,
                pt => pt.TechnologyId,
                t => t.Id,
                (pt, t) => new { pt.ProjectId, Technology = t })
            .ToListAsync(cancellationToken);

        return technologies.ToLookup(x => x.ProjectId, x => x.Technology);
    }

    public async Task ReplaceProjectTechnologiesAsync(Guid ownerId, Guid projectId, IEnumerable<Guid> technologyIds, CancellationToken cancellationToken = default)
    {
        // First verify the project belongs to the owner 
        var projectExists = await _context.Projects.AnyAsync(p => p.OwnerId == ownerId && p.Id == projectId, cancellationToken);
        if (!projectExists) throw new UnauthorizedAccessException("Project does not exist or access denied.");

        var existingJoins = await _context.ProjectTechnologies
            .Where(pt => pt.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        _context.ProjectTechnologies.RemoveRange(existingJoins);

        if (technologyIds != null && technologyIds.Any())
        {
            var distinctIds = technologyIds.Distinct().ToList();
            var newJoins = distinctIds.Select(id => new ProjectTechnology(projectId, id));
            await _context.ProjectTechnologies.AddRangeAsync(newJoins, cancellationToken);
        }
    }

    public async Task RemoveProjectTechnologiesAsync(Guid ownerId, Guid projectId, CancellationToken cancellationToken = default)
    {
        // First verify the project belongs to the owner 
        var projectExists = await _context.Projects.AnyAsync(p => p.OwnerId == ownerId && p.Id == projectId, cancellationToken);
        if (!projectExists) throw new UnauthorizedAccessException("Project does not exist or access denied.");

        var existingJoins = await _context.ProjectTechnologies
            .Where(pt => pt.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        _context.ProjectTechnologies.RemoveRange(existingJoins);
    }
}
