using Backend.Application.Auth;
using Backend.Application.Auth.DTOs;

namespace Backend.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
        {
            try
            {
                var response = await authService.RegisterAsync(request);
                return Results.Created(string.Empty, response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            try
            {
                var response = await authService.LoginAsync(request);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        });
    }
}
