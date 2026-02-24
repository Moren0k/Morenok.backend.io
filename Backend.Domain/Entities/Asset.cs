using Backend.Domain.Common;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

public class Asset : BaseEntity
{
    public Guid OwnerId { get; private set; }
    public AssetProvider Provider { get; private set; }
    public string PublicId { get; private set; }
    public string Url { get; private set; }
    public AssetResourceType ResourceType { get; private set; }

    private Asset() { } // EF Core

    public Asset(
        Guid ownerId,
        AssetProvider provider,
        string publicId,
        string url,
        AssetResourceType resourceType)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("PublicId must not be null or empty.", nameof(publicId));

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url must not be null or empty.", nameof(url));

        OwnerId = ownerId;
        Provider = provider;
        PublicId = publicId;
        Url = url;
        ResourceType = resourceType;
    }
}
