using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 10 login attempts a minute per visitor
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("admin-login", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddHttpClient("admin", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(cfg["Services:Admin:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("X-Service-Key", cfg["Internal:ServiceKey"] ?? "");
    client.Timeout = TimeSpan.FromSeconds(60);   // Render free services can take ~50s to wake up
});

var app = builder.Build();

app.UseRateLimiter();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();