using System.Text.Json.Serialization;
using Pragathi.Admin.Application.Services;
using Pragathi.Admin.Infrastructure;
using Pragathi.Admin.Application.Dtos;
using Pragathi.Admin.Domain.Enums;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is missing.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<IAdminUserService, AdminUserService>();

var app = builder.Build();

// Create the first superuser if the table is empty
using (var scope = app.Services.CreateScope())
{
    var users = scope.ServiceProvider.GetRequiredService<IAdminUserService>();
    var seedId = app.Configuration["Seed:SuperuserId"];
    var seedPassword = app.Configuration["Seed:SuperuserPassword"];

    if ((await users.ListAsync()).Count == 0
        && !string.IsNullOrWhiteSpace(seedId) && !string.IsNullOrEmpty(seedPassword))
    {
        await users.CreateAsync(new CreateUserRequest(seedId, "Super Admin", seedPassword, AdminRole.Superuser));
    }
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();