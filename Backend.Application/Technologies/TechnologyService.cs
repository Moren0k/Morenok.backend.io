using System.Text.RegularExpressions;
using Backend.Application.Contracts.Persistence;
using Backend.Application.Technologies.DTOs;
using Backend.Domain.Entities;

namespace Backend.Application.Technologies;

public class TechnologyService : ITechnologyService
{
    private readonly ITechnologyRepository _technologyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TechnologyService(ITechnologyRepository technologyRepository, IUnitOfWork unitOfWork)
    {
        _technologyRepository = technologyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TechnologyDto>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var technologies = await _technologyRepository.ListAllAsync(cancellationToken);
        return technologies
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .Select(t => new TechnologyDto(t.Id, t.Name, t.Slug, t.CreatedAt, t.UpdatedAt))
            .ToList();
    }

    public async Task<TechnologyDto> CreateAsync(CreateTechnologyRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Technology name cannot be null or empty.");

        // Normalize Name
        var name = Regex.Replace(request.Name.Trim(), @"\s+", " ");

        // Generate Subject
        var slug = GenerateSlug(name);

        var technology = new Technology(name, slug);

        try
        {
            await _technologyRepository.AddAsync(technology, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            // Propagate the specific invalid operation exception thrown by the repository
            throw new ArgumentException("Technology already exists (name or slug).");
        }

        return new TechnologyDto(technology.Id, technology.Name, technology.Slug, technology.CreatedAt, technology.UpdatedAt);
    }

    private static string GenerateSlug(string input)
    {
        var slug = input.ToLowerInvariant().Trim();
        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");
        // Remove invalid characters
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        // Collapse multiple hyphens
        slug = Regex.Replace(slug, @"-+", "-");
        // Trim hyphens from ends
        slug = slug.Trim('-');

        if (string.IsNullOrEmpty(slug))
            throw new ArgumentException("Generated slug is empty. Please provide a valid name.");

        return slug;
    }
}
