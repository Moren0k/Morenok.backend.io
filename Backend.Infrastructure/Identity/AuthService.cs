using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Application.Auth;
using Backend.Application.Auth.DTOs;
using Backend.Application.Common;
using Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        IOptions<JwtOptions> jwtOptions)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            throw new InvalidOperationException("Email already in use.");

        // Derive and normalize slug
        var rawSlug = !string.IsNullOrWhiteSpace(request.PortfolioSlug)
            ? request.PortfolioSlug
            : request.Email.Split('@')[0];

        var normalizedSlug = SlugHelper.NormalizeSlug(rawSlug);

        if (string.IsNullOrEmpty(normalizedSlug))
            throw new ArgumentException("Portfolio slug is invalid or empty after normalization.");

        if (await _context.Users.AnyAsync(u => u.PortfolioSlug == normalizedSlug, cancellationToken))
            throw new InvalidOperationException($"Portfolio slug '{normalizedSlug}' is already in use.");

        var user = new User
        {
            Email = request.Email,
            PortfolioSlug = normalizedSlug
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var token = GenerateJwtToken(user);
        return new AuthResponse(token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult != PasswordVerificationResult.Success)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = GenerateJwtToken(user);
        return new AuthResponse(token);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("owner_id", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
