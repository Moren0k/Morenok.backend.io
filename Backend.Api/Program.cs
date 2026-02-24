using Backend.Api.Endpoints;
using Backend.Api.Middleware;
using Backend.Application.DependencyInjection;
using Backend.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- Size limits ---
const long MaxBodySize = 60 * 1024 * 1024; // 60 MB
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = MaxBodySize);
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = MaxBodySize;
    o.ValueLengthLimit = int.MaxValue;
});

// --- Rate limiting ---
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("portfolio", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 60;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// --- Services ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Middleware pipeline ---
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// --- Endpoints ---
app.MapAuthEndpoints();
app.MapProjectEndpoints();
app.MapTechnologyEndpoints();
app.MapPortfolioEndpoints();
app.MapMeEndpoints();

app.Run();