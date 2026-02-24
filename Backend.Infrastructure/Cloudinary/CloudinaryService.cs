using Backend.Application.Contracts.Infrastructure;
using Backend.Domain.Enums;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Cloudinary;

public class CloudinaryService : ICloudinaryService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinaryOptions> options)
    {
        var config = options.Value;
        var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<(string PublicId, string Url)> UploadImageAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        
        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary Image Upload Error: {uploadResult.Error.Message}");
        }

        return (uploadResult.PublicId, uploadResult.SecureUrl.ToString());
    }

    public async Task<(string PublicId, string Url)> UploadVideoAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var uploadParams = new VideoUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        
        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary Video Upload Error: {uploadResult.Error.Message}");
        }

        return (uploadResult.PublicId, uploadResult.SecureUrl.ToString());
    }

    public async Task DeleteAsync(string publicId, AssetResourceType type, CancellationToken cancellationToken = default)
    {
        var resourceType = type == AssetResourceType.Video ? CloudinaryDotNet.Actions.ResourceType.Video : CloudinaryDotNet.Actions.ResourceType.Image;
        
        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = resourceType
        };

        var result = await _cloudinary.DestroyAsync(deleteParams);
        
        if (result.Error != null)
        {
            throw new Exception($"Cloudinary Delete Error: {result.Error.Message}");
        }
    }
}
