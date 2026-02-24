namespace Backend.Application.Contracts.Persistence;

public record UserRecord(Guid Id, string Email, string PortfolioSlug);
