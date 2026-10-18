using System.Text.Json.Serialization;
using Pragathi.Admin.Application.Services;
using Pragathi.Admin.Infrastructure;
using Pragathi.Admin.Application.Dtos;
using Pragathi.Admin.Domain.Enums;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Pragathi.Admin.Application.Interfaces;
using Pragathi.Admin.Infrastructure.Clients;
using Microsoft.EntityFrameworkCore;
using Pragathi.Admin.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is missing.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<IAdminUserService, AdminUserService>();


var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;   // keep the claim names exactly as in the token
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "pragathi-api",
            ValidateAudience = true,
            ValidAudience = "pragathi-admin",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.Name,   // the names Auth really wrote
            RoleClaimType = ClaimTypes.Role        
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddHttpClient<ILeadsClient, HttpLeadsClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(cfg["Services:Leads:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("X-Service-Key", cfg["Internal:ServiceKey"] ?? "");   // the secret badge
    client.Timeout = TimeSpan.FromSeconds(60);   // free Render services can take ~50s to wake up
});

var app = builder.Build();

// Create the first superuser if the table is empty
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AdminDb>().Database.Migrate();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();