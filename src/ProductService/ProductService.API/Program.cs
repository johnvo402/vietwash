using Microsoft.EntityFrameworkCore;
using ProductService.Application.Commands;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Repositories;
using Micro.Shared.Extensions;
using ProductService.API.Extensions;
using Microsoft.AspNetCore.OData;
using ProductService.Domain.Entities;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.Edm;
using Micro.Shared.Infrastructure.CurrentUserProvider;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers()
                            .AddOData(opt =>
                                opt.Select()
                                   .Filter()
                                   .OrderBy()
                                   .Expand()
                                   .Count()
                                   .SetMaxTop(100).AddRouteComponents("api/v1", GetEdmModel()))
                            .AddJsonOptions(options =>
                            {
                                // Cấu hình camelCase cho JSON
                                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                                // Bỏ qua các trường có giá trị null
                                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                            });
builder.Services.AddSharedSwagger("Product Service API");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUserProvider>();
builder.Services.AddApiVersioningConfig();
builder.Services.AddDataProtectionConfig(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Staging"))
{
    app.UseSharedSwagger();
}
app.MapControllers();

app.UseAuthenticationConfig();
app.Run();
static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EnableLowerCamelCase();
    // Register your entities
    builder.EntitySet<Product>("Products");

    // Ensure the key is recognized
    var product = builder.EntityType<Product>();
    product.HasKey(p => p.Id); // Explicitly define the key

    return builder.GetEdmModel();
}