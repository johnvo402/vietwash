using Micro.Shared.Extensions;
using Micro.Shared.Infrastructure.Policies;
using ProductService.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices(builder);

builder.Services.AddControllers()
       .AddJsonOptions(options =>
       {
           options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
           options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
       });
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseConfigure();
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Staging"))
{
    app.UseSharedSwagger();
}
app.MapControllers();

app.Run();