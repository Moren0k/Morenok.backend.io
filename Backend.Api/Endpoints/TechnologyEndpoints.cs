using Backend.Application.Technologies;
using Backend.Application.Technologies.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Endpoints;

public static class TechnologyEndpoints
{
    public static void MapTechnologyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/technologies").RequireAuthorization();

        group.MapGet("/", async (ITechnologyService technologyService) =>
        {
            try
            {
                var technologies = await technologyService.ListAllAsync();
                return Results.Ok(technologies);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/", async ([FromBody] CreateTechnologyRequest request, ITechnologyService technologyService) =>
        {
            try
            {
                var result = await technologyService.CreateAsync(request);
                return Results.Created($"/api/technologies/{result.Id}", result);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }
}
