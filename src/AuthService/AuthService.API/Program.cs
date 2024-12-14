using AuthService.Application.Commands;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Micro.Shared.Extensions;
using ProductService.API.Extensions;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add data protection configuration
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")))
    .SetApplicationName("AuthService");

// Add services to the container.
// builder.Services.AddOpenApi();
builder.Services.AddControllers()
       .AddJsonOptions(options =>
                            {
                                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                            });
builder.Services.AddSharedSwagger("Auth Service API");

// Cấu hình DbContext cho Identity và User repository
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Identity và IdentityServer
builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Cấu hình MediatR (CQRS)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommandHandler).Assembly));

// Cấu hình Dependency Injection cho các Repository và Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Cấu hình Authorization
builder.Services.AddAuthorization();

builder.Services.AddScoped<ITokenService, TokenService>(sp =>
{
    return new TokenService(sp.GetRequiredService<UserManager<User>>(), builder.Configuration);
});

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddApiVersioningConfig();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "https://d3c2-2001-ee0-5305-c4c0-4c04-8d23-43ae-b514.ngrok-free.app"
                , "http://localhost:5001", "http://auth-service:5001"
            ) // Add your ngrok URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();
app.UseCors("AllowFrontend");
// Ensure the keys directory exists
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "keys"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSharedSwagger();
}

// Cấu hình Authorization cho các yêu cầu bảo vệ
app.UseAuthenticationConfig();
// Map các Controllers
app.MapControllers();

app.Run();
