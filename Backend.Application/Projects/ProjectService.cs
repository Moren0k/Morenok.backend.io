using Backend.Application.Assets;
using Backend.Application.Contracts.Persistence;
using Backend.Application.Projects.DTOs;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Projects;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITechnologyRepository _technologyRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IAssetOrchestrator _assetOrchestrator;
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(
        IProjectRepository projectRepository, 
        ITechnologyRepository technologyRepository,
        IAssetRepository assetRepository,
        IAssetOrchestrator assetOrchestrator,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _technologyRepository = technologyRepository;
        _assetRepository = assetRepository;
        _assetOrchestrator = assetOrchestrator;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, CancellationToken cancellationToken = default)
    {
        ValidateUrls(dto.LiveUrl, dto.RepoUrl);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            int finalDisplayOrder = dto.DisplayOrder ?? 0;

            if (dto.IsPinned)
            {
                finalDisplayOrder = 0;
                var existingPinnedId = await _projectRepository.GetPinnedProjectIdAsync(dto.OwnerId, cancellationToken);
                if (existingPinnedId.HasValue)
                {
                    var existingPinned = await _projectRepository.GetByIdAsync(dto.OwnerId, existingPinnedId.Value, cancellationToken);
                    if (existingPinned != null)
                    {
                        existingPinned.Unpin(1);
                        await _projectRepository.ShiftDisplayOrdersAsync(dto.OwnerId, 1, 1, existingPinned.Id, cancellationToken);
                        await _projectRepository.UpdateAsync(existingPinned, cancellationToken);
                    }
                }
            }
            else
            {
                var maxOrder = await _projectRepository.GetMaxDisplayOrderAsync(dto.OwnerId, cancellationToken);
                if (dto.DisplayOrder == null || dto.DisplayOrder < 1 || dto.DisplayOrder > maxOrder + 1)
                {
                    finalDisplayOrder = maxOrder + 1;
                }
                else
                {
                    finalDisplayOrder = dto.DisplayOrder.Value;
                    await _projectRepository.ShiftDisplayOrdersAsync(dto.OwnerId, finalDisplayOrder, 1, null, cancellationToken);
                }
            }

            var project = new Project(
                ownerId: dto.OwnerId,
                name: dto.Name,
                shortDescription: dto.ShortDescription,
                coverAssetId: dto.CoverAssetId,
                longDescription: dto.LongDescription,
                liveUrl: dto.LiveUrl,
                repoUrl: dto.RepoUrl,
                status: dto.Status,
                isPinned: dto.IsPinned,
                displayOrder: finalDisplayOrder,
                demoVideoAssetId: dto.DemoVideoAssetId
            );

            await _projectRepository.AddAsync(project, cancellationToken);
            await _projectRepository.NormalizeDisplayOrdersAsync(dto.OwnerId, cancellationToken);
            
            if (dto.TechnologyIds != null && dto.TechnologyIds.Any())
            {
                bool allExist = await _technologyRepository.ExistsAllAsync(dto.TechnologyIds, cancellationToken);
                if (!allExist) throw new ArgumentException("One or more technology IDs are invalid.");
                
                await _projectRepository.ReplaceProjectTechnologiesAsync(dto.OwnerId, project.Id, dto.TechnologyIds, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            return MapToDto(project);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ProjectDto> UpdateProjectAsync(Guid ownerId, Guid projectId, UpdateProjectDto dto, CancellationToken cancellationToken = default)
    {
        ValidateUrls(dto.LiveUrl, dto.RepoUrl);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var project = await _projectRepository.GetByIdAsync(ownerId, projectId, cancellationToken);
            if (project == null) throw new Exception("Project not found");

            bool oldIsPinned = project.IsPinned;
            int oldDisplayOrder = project.DisplayOrder;

            // Apply data updates
            project.UpdateDetails(
                dto.Name,
                dto.ShortDescription,
                dto.LongDescription,
                dto.LiveUrl,
                dto.RepoUrl,
                dto.Status,
                dto.CoverAssetId,
                dto.DemoVideoAssetId
            );

            // Scenario 1: Changing Pinned Status
            if (dto.IsPinned && !oldIsPinned)
            {
                // Unpinning old project if exists
                var existingPinnedId = await _projectRepository.GetPinnedProjectIdAsync(ownerId, cancellationToken);
                if (existingPinnedId.HasValue && existingPinnedId.Value != projectId)
                {
                    var existingPinned = await _projectRepository.GetByIdAsync(ownerId, existingPinnedId.Value, cancellationToken);
                    if (existingPinned != null)
                    {
                        existingPinned.Unpin(1);
                        await _projectRepository.ShiftDisplayOrdersAsync(ownerId, 1, 1, existingPinned.Id, cancellationToken);
                        await _projectRepository.UpdateAsync(existingPinned, cancellationToken);
                    }
                }
                
                project.Pin();
            }
            else if (!dto.IsPinned && oldIsPinned)
            {
                // Previously pinned, now unpinned
                var maxOrder = await _projectRepository.GetMaxDisplayOrderAsync(ownerId, cancellationToken);
                int finalDisplayOrder;
                if (dto.DisplayOrder == null || dto.DisplayOrder < 1)
                {
                    finalDisplayOrder = maxOrder + 1;
                }
                else
                {
                    finalDisplayOrder = dto.DisplayOrder.Value;
                    await _projectRepository.ShiftDisplayOrdersAsync(ownerId, finalDisplayOrder, 1, project.Id, cancellationToken);
                }
                project.Unpin(finalDisplayOrder);
            }
            // Scenario 2: Both False, Changing DisplayOrder
            else if (!dto.IsPinned && !oldIsPinned && dto.DisplayOrder.HasValue && dto.DisplayOrder.Value != oldDisplayOrder)
            {
                if (dto.DisplayOrder.Value >= 1)
                {
                    int newOrder = dto.DisplayOrder.Value;
                    await _projectRepository.ShiftDisplayOrdersAsync(ownerId, newOrder, 1, project.Id, cancellationToken);
                    project.SetDisplayOrder(newOrder);
                }
            }

            await _projectRepository.UpdateAsync(project, cancellationToken);
            await _projectRepository.NormalizeDisplayOrdersAsync(ownerId, cancellationToken);
            
            if (dto.TechnologyIds != null)
            {
                if (dto.TechnologyIds.Any())
                {
                    bool allExist = await _technologyRepository.ExistsAllAsync(dto.TechnologyIds, cancellationToken);
                    if (!allExist) throw new ArgumentException("One or more technology IDs are invalid.");
                }
                
                await _projectRepository.ReplaceProjectTechnologiesAsync(ownerId, project.Id, dto.TechnologyIds, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            return MapToDto(project);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteProjectAsync(Guid ownerId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(ownerId, projectId, cancellationToken);
        if (project == null) return;

        // Capture asset IDs before deletion
        var coverId = project.CoverAssetId;
        var demoId = project.DemoVideoAssetId;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _projectRepository.RemoveProjectTechnologiesAsync(ownerId, projectId, cancellationToken);
            await _projectRepository.DeleteAsync(project, cancellationToken);
            await _projectRepository.NormalizeDisplayOrdersAsync(ownerId, cancellationToken);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        // Post-commit asset cleanup
        await _assetOrchestrator.DeleteAssetBothAsync(ownerId, coverId, cancellationToken);
        if (demoId.HasValue)
        {
            await _assetOrchestrator.DeleteAssetBothAsync(ownerId, demoId.Value, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<ProjectDto>> GetProjectsForAdminAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.ListAllAsync(ownerId, cancellationToken);
        if (!projects.Any()) return Array.Empty<ProjectDto>();

        var projectIds = projects.Select(p => p.Id).ToList();
        var technologiesLookup = await _projectRepository.GetTechnologiesForProjectsAsync(ownerId, projectIds, cancellationToken);
        
        return projects.Select(project => {
            var techDtos = technologiesLookup[project.Id]
                .Select(t => new Backend.Application.Technologies.DTOs.TechnologyDto(t.Id, t.Name, t.Slug, t.CreatedAt, t.UpdatedAt))
                .ToList();
            
            return MapToDto(project, null, null) with { Technologies = techDtos };
        }).ToList();
    }

    public async Task<IReadOnlyList<ProjectDto>> GetPublishedProjectsAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.ListPublishedAsync(ownerId, cancellationToken);
        if (!projects.Any()) return Array.Empty<ProjectDto>();

        var projectIds = projects.Select(p => p.Id).ToList();
        
        // Bulk fetch technologies
        var technologiesLookup = await _projectRepository.GetTechnologiesForProjectsAsync(ownerId, projectIds, cancellationToken);
        
        // Bulk fetch assets to get URLs
        var assetIds = projects.Select(p => p.CoverAssetId)
            .Concat(projects.Where(p => p.DemoVideoAssetId.HasValue).Select(p => p.DemoVideoAssetId!.Value))
            .Distinct()
            .ToList();
            
        var assets = await _assetRepository.GetByIdsAsync(ownerId, assetIds, cancellationToken);
        var assetsLookup = assets.ToDictionary(a => a.Id);

        return projects.Select(project => {
            var techDtos = technologiesLookup[project.Id]
                .Select(t => new Backend.Application.Technologies.DTOs.TechnologyDto(t.Id, t.Name, t.Slug, t.CreatedAt, t.UpdatedAt))
                .ToList();
            
            assetsLookup.TryGetValue(project.CoverAssetId, out var cover);
            Asset? demo = null;
            if (project.DemoVideoAssetId.HasValue)
            {
                assetsLookup.TryGetValue(project.DemoVideoAssetId.Value, out demo);
            }
            
            return MapToDto(project, cover, demo) with { Technologies = techDtos };
        }).ToList();
    }

    private static void ValidateUrls(params string?[] urls)
    {
        foreach (var url in urls)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) || 
                   (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    throw new ArgumentException($"Invalid URL format: {url}");
                }
            }
        }
    }

    private static ProjectDto MapToDto(Project p, Asset? coverAsset = null, Asset? demoVideoAsset = null)
    {
        return new ProjectDto(
            p.Id, p.OwnerId, p.Name, p.ShortDescription, p.LongDescription, p.LiveUrl, p.RepoUrl,
            p.Status, p.IsPinned, p.DisplayOrder, p.CoverAssetId, p.DemoVideoAssetId, 
            coverAsset?.Url, demoVideoAsset?.Url, 
            p.CreatedAt, p.UpdatedAt
        );
    }
}
