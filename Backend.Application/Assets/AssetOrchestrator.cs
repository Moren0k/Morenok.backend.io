using Backend.Application.Contracts.Infrastructure;
using Backend.Application.Contracts.Persistence;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Backend.Application.Assets;

public interface IAssetOrchestrator
{
    Task<Asset> CreateAssetFromUploadAsync(Guid ownerId, IFormFile file, AssetResourceType type, CancellationToken cancellationToken = default);
    Task DeleteAssetBothAsync(Guid ownerId, Guid assetId, CancellationToken cancellationToken = default);
}

public class AssetOrchestrator : IAssetOrchestrator
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssetOrchestrator(
        ICloudinaryService cloudinaryService, 
        IAssetRepository assetRepository,
        IUnitOfWork unitOfWork)
    {
        _cloudinaryService = cloudinaryService;
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Asset> CreateAssetFromUploadAsync(Guid ownerId, IFormFile file, AssetResourceType type, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        // Guardrails
        if (type == AssetResourceType.Image && file.Length > 10 * 1024 * 1024)
            throw new InvalidOperationException("Image exceeds 10MB limit.");
        
        if (type == AssetResourceType.Video && file.Length > 50 * 1024 * 1024)
            throw new InvalidOperationException("Video exceeds 50MB limit.");

        string publicId = null;
        string url = null;

        var folder = $"morenok/{ownerId}";

        using var stream = file.OpenReadStream();
        try
        {
            if (type == AssetResourceType.Image)
            {
                (publicId, url) = await _cloudinaryService.UploadImageAsync(stream, file.FileName, folder, cancellationToken);
            }
            else
            {
                (publicId, url) = await _cloudinaryService.UploadVideoAsync(stream, file.FileName, folder, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to upload to Cloudinary: {ex.Message}", ex);
        }

        try
        {
            var asset = new Asset(
                ownerId: ownerId,
                provider: AssetProvider.Cloudinary,
                resourceType: type,
                publicId: publicId,
                url: url
            );

            await _assetRepository.AddAsync(asset, cancellationToken);
            
            // Note: SaveChanges is intentionally NOT called here.
            // It assumes the caller (e.g. ProjectEndpoints) wraps this within a UnitOfWork
            // and calls SaveChanges once all entities (Asset + Project) are Added/Updated.
            
            return asset;
        }
        catch (Exception ex)
        {
            // Compensation: If adding to DB transaction scope fails, immediately delete from Cloudinary
            try
            {
                await _cloudinaryService.DeleteAsync(publicId, type, CancellationToken.None);
            }
            catch
            {
                // Log failure to delete orphaned asset, but throw original exception
            }
            
            throw new InvalidOperationException($"Failed to create asset entity: {ex.Message}", ex);
        }
    }

    public async Task DeleteAssetBothAsync(Guid ownerId, Guid assetId, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByIdAsync(ownerId, assetId, cancellationToken);
        if (asset == null)
            return;

        // DB dictates truth. We must commit deleting the asset entity before deleting from Cloudinary.
        // If DB commit fails, Cloudinary asset still exists, transaction rolls back, data is safe.
        await _assetRepository.DeleteAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _cloudinaryService.DeleteAsync(asset.PublicId, asset.ResourceType, cancellationToken);
        }
        catch (Exception)
        {
            // Log this scenario if a logger is available: It means DB is clean but Cloudinary holds an orphaned file.
            // We ignore throwing because preventing the user request from returning 200 due to a garbage cleanup failure is hostile.
        }
    }
}
