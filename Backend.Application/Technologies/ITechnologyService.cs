using Backend.Application.Technologies.DTOs;

namespace Backend.Application.Technologies;

public interface ITechnologyService
{
    Task<IReadOnlyList<TechnologyDto>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<TechnologyDto> CreateAsync(CreateTechnologyRequest request, CancellationToken cancellationToken = default);
}
