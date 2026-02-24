using System.Security.Claims;
using Backend.Application.Projects;
using Backend.Application.Projects.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IProjectService projectService) =>
        {
            try
            {
                var ownerId = GetOwnerId(user);
                var projects = await projectService.GetPublishedProjectsAsync(ownerId);
                return Results.Ok(projects);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapGet("/admin", async (ClaimsPrincipal user, IProjectService projectService) =>
        {
            try
            {
                var ownerId = GetOwnerId(user);
                var projects = await projectService.GetProjectsForAdminAsync(ownerId);
                return Results.Ok(projects);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/", async (
            HttpContext context, 
            ClaimsPrincipal user, 
            IProjectService projectService,
            Backend.Application.Assets.IAssetOrchestrator assetOrchestrator) =>
        {
            try
            {
                var ownerId = GetOwnerId(user);
                var form = await context.Request.ReadFormAsync();
                
                var coverFile = form.Files["cover"];
                if (coverFile == null)
                    return Results.BadRequest(new { error = "Cover image is required." });

                var demoVideoFile = form.Files["demoVideo"];

                // Read other parameters
                var name = form["name"].ToString();
                var shortDescription = form["shortDescription"].ToString();
                var longDescription = form["longDescription"].ToString();
                var liveUrl = form["liveUrl"].ToString();
                var repoUrl = form["repoUrl"].ToString();
                var statusStr = form["status"].ToString();
                var isPinnedStr = form["isPinned"].ToString();
                var displayOrderStr = form["displayOrder"].ToString();

                var status = Enum.TryParse<Backend.Domain.Enums.ProjectStatus>(statusStr, out var s) ? s : Backend.Domain.Enums.ProjectStatus.Draft;
                var isPinned = bool.TryParse(isPinnedStr, out var p) && p;
                int? displayOrder = int.TryParse(displayOrderStr, out var d) ? d : null;

                // Step 1: Upload cover
                var coverAsset = await assetOrchestrator.CreateAssetFromUploadAsync(ownerId, coverFile, Backend.Domain.Enums.AssetResourceType.Image);
                var demoVideoAsset = demoVideoFile != null ? await assetOrchestrator.CreateAssetFromUploadAsync(ownerId, demoVideoFile, Backend.Domain.Enums.AssetResourceType.Video) : null;

                // Extract technology ids if present
                var technologyIdsStr = form["technologyIds"].ToString();
                var technologyIds = new List<Guid>();
                if (!string.IsNullOrEmpty(technologyIdsStr))
                {
                    technologyIds = technologyIdsStr.Split(',')
                                    .Select(id => Guid.TryParse(id.Trim(), out var parsed) ? parsed : Guid.Empty)
                                    .Where(id => id != Guid.Empty)
                                    .ToList();
                }

                try
                {
                    var dto = new CreateProjectDto(
                        ownerId,
                        name,
                        shortDescription,
                        string.IsNullOrEmpty(longDescription) ? null : longDescription,
                        string.IsNullOrEmpty(liveUrl) ? null : liveUrl,
                        string.IsNullOrEmpty(repoUrl) ? null : repoUrl,
                        status,
                        isPinned,
                        displayOrder,
                        coverAsset.Id,
                        demoVideoAsset?.Id,
                        technologyIds.Any() ? technologyIds : null
                    );

                    var result = await projectService.CreateProjectAsync(dto);
                    return Results.Created($"/api/projects/{result.Id}", result);
                }
                catch
                {
                    // Compensation: Delete created assets if project service fails (e.g validation)
                    await assetOrchestrator.DeleteAssetBothAsync(ownerId, coverAsset.Id);
                    if (demoVideoAsset != null)
                        await assetOrchestrator.DeleteAssetBothAsync(ownerId, demoVideoAsset.Id);
                        
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPut("/{projectId:guid}", async (
            Guid projectId, 
            HttpContext context, 
            ClaimsPrincipal user, 
            IProjectService projectService,
            Backend.Application.Contracts.Persistence.IProjectRepository projectRepository,
            Backend.Application.Assets.IAssetOrchestrator assetOrchestrator) =>
        {
            try
            {
                var ownerId = GetOwnerId(user);
                
                // 1. Fetch current domain state to pull current assets accurately
                var oldProject = await projectRepository.GetByIdAsync(ownerId, projectId);
                if (oldProject == null)
                    return Results.NotFound();
                    
                var form = await context.Request.ReadFormAsync();

                // These params might be partial or full depending on client, assuming full mapping for DTO
                var name = form["name"].ToString();
                var shortDescription = form["shortDescription"].ToString();
                var longDescription = form["longDescription"].ToString();
                var liveUrl = form["liveUrl"].ToString();
                var repoUrl = form["repoUrl"].ToString();
                var statusStr = form["status"].ToString();
                var isPinnedStr = form["isPinned"].ToString();
                var displayOrderStr = form["displayOrder"].ToString();
                var removeDemoVideoStr = form["removeDemoVideo"].ToString();

                var status = Enum.TryParse<Backend.Domain.Enums.ProjectStatus>(statusStr, out var s) ? s : Backend.Domain.Enums.ProjectStatus.Draft;
                var isPinned = bool.TryParse(isPinnedStr, out var p) && p;
                int? displayOrder = int.TryParse(displayOrderStr, out var d) ? d : null;
                var removeDemoVideo = bool.TryParse(removeDemoVideoStr, out var rmv) && rmv;

                var coverFile = form.Files["cover"];
                var demoVideoFile = form.Files["demoVideo"];

                Backend.Domain.Entities.Asset newCoverAsset = null;
                Backend.Domain.Entities.Asset newVideoAsset = null;

                var finalCoverId = oldProject.CoverAssetId;
                var finalVideoId = oldProject.DemoVideoAssetId;

                if (coverFile != null)
                {
                    newCoverAsset = await assetOrchestrator.CreateAssetFromUploadAsync(ownerId, coverFile, Backend.Domain.Enums.AssetResourceType.Image);
                    finalCoverId = newCoverAsset.Id;
                }

                if (demoVideoFile != null)
                {
                    newVideoAsset = await assetOrchestrator.CreateAssetFromUploadAsync(ownerId, demoVideoFile, Backend.Domain.Enums.AssetResourceType.Video);
                    finalVideoId = newVideoAsset.Id;
                }

                if (removeDemoVideo && demoVideoFile == null)
                {
                    finalVideoId = null;
                }

                var technologyIdsStr = form["technologyIds"].ToString();
                var technologyIds = new List<Guid>();
                if (!string.IsNullOrEmpty(technologyIdsStr))
                {
                    technologyIds = technologyIdsStr.Split(',')
                                    .Select(id => Guid.TryParse(id.Trim(), out var parsed) ? parsed : Guid.Empty)
                                    .Where(id => id != Guid.Empty)
                                    .ToList();
                }

                try
                {
                    var dto = new UpdateProjectDto(
                        name,
                        shortDescription,
                        string.IsNullOrEmpty(longDescription) ? null : longDescription,
                        string.IsNullOrEmpty(liveUrl) ? null : liveUrl,
                        string.IsNullOrEmpty(repoUrl) ? null : repoUrl,
                        status,
                        isPinned,
                        displayOrder,
                        finalCoverId,
                        finalVideoId,
                        technologyIds.Any() ? technologyIds : null
                    );

                    var result = await projectService.UpdateProjectAsync(ownerId, projectId, dto);

                    // Post-commit cleanup: if update succeeded, remove old assets derived from state precisely
                    if (newCoverAsset != null)
                        await assetOrchestrator.DeleteAssetBothAsync(ownerId, oldProject.CoverAssetId);
                        
                    if (newVideoAsset != null && oldProject.DemoVideoAssetId.HasValue)
                        await assetOrchestrator.DeleteAssetBothAsync(ownerId, oldProject.DemoVideoAssetId.Value);
                    else if (removeDemoVideo && newVideoAsset == null && oldProject.DemoVideoAssetId.HasValue)
                        await assetOrchestrator.DeleteAssetBothAsync(ownerId, oldProject.DemoVideoAssetId.Value);

                    return Results.Ok(result);
                }
                catch
                {
                    // Compensation: rollout new uploaded assets because the project update failed
                    if (newCoverAsset != null) await assetOrchestrator.DeleteAssetBothAsync(ownerId, newCoverAsset.Id);
                    if (newVideoAsset != null) await assetOrchestrator.DeleteAssetBothAsync(ownerId, newVideoAsset.Id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Project not found") return Results.NotFound();
                return Results.BadRequest(new { error = ex.Message });
            }
        });
        
        group.MapDelete("/{projectId:guid}", async (Guid projectId, ClaimsPrincipal user, IProjectService projectService) =>
        {
            try
            {
                var ownerId = GetOwnerId(user);
                await projectService.DeleteProjectAsync(ownerId, projectId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                if (ex.Message == "Project not found") return Results.NotFound();
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }

    private static Guid GetOwnerId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst("owner_id")?.Value;
        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var ownerId))
        {
            throw new UnauthorizedAccessException("Invalid or missing owner_id claim.");
        }
        return ownerId;
    }
}
