using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace PragathiApi.Auth.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminAuthController(IConfiguration config) : ControllerBase
{
    public record LoginRequest(string? Id, string? Password);

    [HttpPost("login")]
    [EnableRateLimiting("admin-login")]
    public IActionResult Login(LoginRequest body)
    {
        var id = config["Admin:Id"];
        var password = config["Admin:Password"];
        var key = config["Jwt:Key"];

        // Not configured = login switched off.
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(key))
            return NotFound();

        if (!Same(body.Id, id) || !Same(body.Password, password))
            return Unauthorized();

        var expires = DateTime.UtcNow.AddHours(2);
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: new[] { new Claim(ClaimTypes.Name, id), new Claim(ClaimTypes.Role, "admin") },
            expires: expires,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), expiresAt = expires });
    }

    private static bool Same(string? given, string expected)
    {
        var a = SHA256.HashData(Encoding.UTF8.GetBytes(given ?? ""));
        var b = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}