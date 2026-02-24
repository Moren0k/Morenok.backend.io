using Backend.Infrastructure.Cloudinary;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<CloudinaryOptions>(configuration.GetSection("Cloudinary"));
        services.Configure<Backend.Infrastructure.Identity.JwtOptions>(configuration.GetSection("Jwt"));

        services.AddScoped<Backend.Application.Contracts.Persistence.IProjectRepository, Backend.Infrastructure.Repositories.ProjectRepository>();
        services.AddScoped<Backend.Application.Contracts.Persistence.ITechnologyRepository, Backend.Infrastructure.Repositories.TechnologyRepository>();
        services.AddScoped<Backend.Application.Contracts.Persistence.IAssetRepository, Backend.Infrastructure.Repositories.AssetRepository>();
        services.AddScoped<Backend.Application.Contracts.Persistence.IUnitOfWork, Backend.Infrastructure.Repositories.UnitOfWork>();
        services.AddScoped<Backend.Application.Contracts.Persistence.IUserRepository, Backend.Infrastructure.Repositories.UserRepository>();

        services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<Backend.Infrastructure.Identity.User>, Microsoft.AspNetCore.Identity.PasswordHasher<Backend.Infrastructure.Identity.User>>();
        services.AddScoped<Backend.Application.Auth.IAuthService, Backend.Infrastructure.Identity.AuthService>();
        services.AddScoped<Backend.Application.Contracts.Infrastructure.ICloudinaryService, Backend.Infrastructure.Cloudinary.CloudinaryService>();

        return services;
    }
}
