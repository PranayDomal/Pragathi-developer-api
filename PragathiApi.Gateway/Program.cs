var builder = WebApplication.CreateBuilder(args);

// Routes and destinations come from appsettings.json ("ReverseProxy")
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// CORS lives here, at the single entry point, so the services don't need it
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(origins).AllowAnyHeader().WithMethods("GET", "POST", "PUT", "DELETE")));

var app = builder.Build();

app.UseCors();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapReverseProxy();

app.Run();