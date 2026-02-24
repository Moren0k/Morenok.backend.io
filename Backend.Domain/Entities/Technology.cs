using Backend.Domain.Common;

namespace Backend.Domain.Entities;

public class Technology : BaseEntity
{
    public string Name { get; private set; }
    public string Slug { get; private set; }

    private Technology() { } // EF Core

    public Technology(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must not be null or empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug must not be null or empty.", nameof(slug));

        Name = name;
        Slug = slug.ToLowerInvariant();
    }
}
