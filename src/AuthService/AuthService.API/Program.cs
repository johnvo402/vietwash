using AuthService.Application.Commands;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Micro.Shared.Extensions;
using ProductService.API.Extensions;


var builder = WebApplication.CreateBuilder(args);


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
builder.Services.AddDataProtectionConfig(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Staging"))
{
    app.UseSharedSwagger();
}

// Cấu hình Authorization cho các yêu cầu bảo vệ
app.UseAuthenticationConfig();
// Map các Controllers
app.MapControllers();

app.Run();
