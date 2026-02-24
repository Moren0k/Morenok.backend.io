using Backend.Application.Contracts.Persistence;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> GetOwnerIdByPortfolioSlugAsync(string normalizedSlug, CancellationToken cancellationToken = default)
    {
        var id = await _context.Users
            .AsNoTracking()
            .Where(u => u.PortfolioSlug == normalizedSlug)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return id;
    }

    public Task<UserRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserRecord(u.Id, u.Email, u.PortfolioSlug))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
