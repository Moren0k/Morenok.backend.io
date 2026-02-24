using Backend.Application.Common;
using Backend.Application.Contracts.Persistence;
using Backend.Application.Projects;

namespace Backend.Api.Endpoints;

public static class PortfolioEndpoints
{
    public static void MapPortfolioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/portfolio")
            .RequireRateLimiting("portfolio");

        group.MapGet("/{portfolioSlug}/projects", async (
            string portfolioSlug,
            IUserRepository userRepository,
            IProjectService projectService) =>
        {
            var normalizedSlug = SlugHelper.NormalizeSlug(portfolioSlug);
            if (string.IsNullOrEmpty(normalizedSlug))
                return Results.NotFound();

            var ownerId = await userRepository.GetOwnerIdByPortfolioSlugAsync(normalizedSlug);
            if (ownerId is null)
                return Results.NotFound();

            var projects = await projectService.GetPublishedProjectsAsync(ownerId.Value);
            return Results.Ok(projects);
        });
    }
}
