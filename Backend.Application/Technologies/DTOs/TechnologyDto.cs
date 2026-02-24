namespace Backend.Application.Technologies.DTOs;

public record TechnologyDto(
    Guid Id,
    string Name,
    string Slug,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
