using Backend.Application.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<Backend.Application.Technologies.ITechnologyService, Backend.Application.Technologies.TechnologyService>();
        services.AddScoped<Backend.Application.Assets.IAssetOrchestrator, Backend.Application.Assets.AssetOrchestrator>();

        return services;
    }
}
