using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace PragathiApi.Auth.Controllers;

public record LoginRequest(string? Id, string? Password);
public record AdminVerifyResult(string LoginId, string Name, string Role);

[ApiController]
[Route("api/admin")]
public class AdminAuthController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public AdminAuthController(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    [HttpPost("login")]
    [EnableRateLimiting("admin-login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var key = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(key)) return NotFound();   // login switched off if no signing key

        if (string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrEmpty(request.Password))
            return Unauthorized(new { message = "Invalid ID or password." });

        // Ask the Admin service: is this person real, and what is their role?
        HttpResponseMessage response;
        try
        {
            var client = _http.CreateClient("admin");
            response = await client.PostAsJsonAsync("internal/auth/verify",
                new { loginId = request.Id, password = request.Password });
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return StatusCode(503, new { message = "Login service is unavailable. Please try again." });
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return Unauthorized(new { message = "Invalid ID or password." });
        if (response.StatusCode == HttpStatusCode.Forbidden)
            return StatusCode(403, new { message = "This account is switched off. Please contact your administrator." });
        if (!response.IsSuccessStatusCode)
            return StatusCode(503, new { message = "Login service is unavailable. Please try again." });

        var user = await response.Content.ReadFromJsonAsync<AdminVerifyResult>();
        if (user is null) return StatusCode(503, new { message = "Login service is unavailable. Please try again." });

        // Build the token, now with the real role (Viewer / Editor / Superuser)
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.LoginId),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var expires = DateTime.UtcNow.AddHours(2);
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken("pragathi-api", "pragathi-admin", claims,
            expires: expires, signingCredentials: creds);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt = expires,
            role = user.Role,
            name = user.Name
        });
    }
}