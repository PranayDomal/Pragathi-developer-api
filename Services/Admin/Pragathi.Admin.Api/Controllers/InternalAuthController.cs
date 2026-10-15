using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Pragathi.Admin.Application.Dtos;
using Pragathi.Admin.Application.Services;

namespace Pragathi.Admin.Api.Controllers;

[ApiController]
[Route("internal/auth")]
public class InternalAuthController : ControllerBase
{
    private readonly IAdminUserService _users;
    private readonly IConfiguration _config;

    public InternalAuthController(IAdminUserService users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    // Only the Auth service calls this. It must send the secret key in a header.
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyCredentialsRequest request)
    {
        var expected = _config["Internal:ServiceKey"];
        var given = Request.Headers["X-Service-Key"].ToString();
        if (string.IsNullOrEmpty(expected) || !Same(given, expected)) return Unauthorized();

        var result = await _users.VerifyAsync(request);
        return result is null ? Unauthorized() : Ok(result);
    }

    private static bool Same(string a, string b)
    {
        var x = SHA256.HashData(Encoding.UTF8.GetBytes(a));
        var y = SHA256.HashData(Encoding.UTF8.GetBytes(b));
        return CryptographicOperations.FixedTimeEquals(x, y);
    }
}