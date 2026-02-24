using Backend.Domain.Common;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

public class Project : BaseEntity
{
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public string ShortDescription { get; private set; }
    public string? LongDescription { get; private set; }
    public string? LiveUrl { get; private set; }
    public string? RepoUrl { get; private set; }
    public ProjectStatus Status { get; private set; }
    public bool IsPinned { get; private set; }
    public int DisplayOrder { get; private set; }

    public Guid CoverAssetId { get; private set; }
    public Guid? DemoVideoAssetId { get; private set; }

    private Project() { } // EF Core

    public Project(
        Guid ownerId,
        string name,
        string shortDescription,
        Guid coverAssetId,
        string? longDescription = null,
        string? liveUrl = null,
        string? repoUrl = null,
        ProjectStatus status = ProjectStatus.Draft,
        bool isPinned = false,
        int displayOrder = 1,
        Guid? demoVideoAssetId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be null or empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(shortDescription))
            throw new ArgumentException("ShortDescription must not be null or empty.", nameof(shortDescription));

        if (displayOrder < 0)
            throw new ArgumentException("DisplayOrder must be >= 0.", nameof(displayOrder));

        if (isPinned && displayOrder != 0)
            throw new ArgumentException("If IsPinned is true, DisplayOrder must be 0.", nameof(displayOrder));

        if (!isPinned && displayOrder < 1)
            throw new ArgumentException("If IsPinned is false, DisplayOrder must be >= 1.", nameof(displayOrder));

        OwnerId = ownerId;
        Name = name;
        ShortDescription = shortDescription;
        CoverAssetId = coverAssetId;
        LongDescription = longDescription;
        LiveUrl = liveUrl;
        RepoUrl = repoUrl;
        Status = status;
        IsPinned = isPinned;
        DisplayOrder = displayOrder;
        DemoVideoAssetId = demoVideoAssetId;
    }

    public void UpdateDetails(
        string name,
        string shortDescription,
        string? longDescription,
        string? liveUrl,
        string? repoUrl,
        ProjectStatus status,
        Guid coverAssetId,
        Guid? demoVideoAssetId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be null or empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(shortDescription))
            throw new ArgumentException("ShortDescription must not be null or empty.", nameof(shortDescription));

        Name = name;
        ShortDescription = shortDescription;
        LongDescription = longDescription;
        LiveUrl = liveUrl;
        RepoUrl = repoUrl;
        Status = status;
        CoverAssetId = coverAssetId;
        DemoVideoAssetId = demoVideoAssetId;
    }

    public void Pin()
    {
        IsPinned = true;
        DisplayOrder = 0;
    }

    public void Unpin(int newDisplayOrder)
    {
        if (newDisplayOrder < 1)
            throw new ArgumentException("newDisplayOrder must be >= 1.", nameof(newDisplayOrder));
            
        IsPinned = false;
        DisplayOrder = newDisplayOrder;
    }

    public void SetDisplayOrder(int newDisplayOrder)
    {
        if (IsPinned)
            throw new InvalidOperationException("Cannot set DisplayOrder on a pinned project. Unpin it first.");
            
        if (newDisplayOrder < 1)
            throw new ArgumentException("newDisplayOrder must be >= 1.", nameof(newDisplayOrder));
            
        DisplayOrder = newDisplayOrder;
    }
}