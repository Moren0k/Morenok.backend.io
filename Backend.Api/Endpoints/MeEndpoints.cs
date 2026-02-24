using System.Security.Claims;
using Backend.Application.Contracts.Persistence;

namespace Backend.Api.Endpoints;

public static class MeEndpoints
{
    public static void MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/me", async (ClaimsPrincipal user, IUserRepository userRepository) =>
        {
            var ownerIdClaim = user.FindFirst("owner_id")?.Value;
            if (string.IsNullOrEmpty(ownerIdClaim) || !Guid.TryParse(ownerIdClaim, out var ownerId))
                return Results.Problem(title: "Unauthorized", detail: "Invalid or missing owner_id claim.", statusCode: StatusCodes.Status401Unauthorized);

            var dbUser = await userRepository.GetByIdAsync(ownerId);
            if (dbUser is null)
                return Results.Problem(title: "Not Found", detail: "User not found.", statusCode: StatusCodes.Status404NotFound);

            return Results.Ok(new
            {
                dbUser.Id,
                dbUser.Email,
                dbUser.PortfolioSlug
            });
        })
        .RequireAuthorization();
    }
}
