using Backend.Domain.Enums;

namespace Backend.Application.Contracts.Infrastructure;

public interface ICloudinaryService
{
    Task<(string PublicId, string Url)> UploadImageAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task<(string PublicId, string Url)> UploadVideoAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task DeleteAsync(string publicId, AssetResourceType type, CancellationToken cancellationToken = default);
}
