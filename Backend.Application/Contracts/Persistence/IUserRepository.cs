namespace Backend.Application.Contracts.Persistence;

public interface IUserRepository
{
    Task<Guid?> GetOwnerIdByPortfolioSlugAsync(string normalizedSlug, CancellationToken cancellationToken = default);
    Task<UserRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
